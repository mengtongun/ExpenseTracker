using AutoMapper;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Contracts.RecurringExpenses;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using System;

namespace ExpenseTracker.Application.Services.RecurringExpenses;

public class RecurringExpenseService : IRecurringExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RecurringExpenseService(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RecurringExpenseDto> CreateAsync(int userId, CreateRecurringExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var categoryId = await ResolveCategoryIdAsync(userId, request.CategoryId, cancellationToken);

        var recurringExpense = new RecurringExpense
        {
            UserId = userId,
            CategoryId = categoryId,
            Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Description = request.Description?.Trim(),
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextOccurrence = CalculateInitialNextOccurrence(request.StartDate, request.Frequency, _dateTimeProvider.Today),
            IsActive = true
        };

        await _unitOfWork.RecurringExpenses.AddAsync(recurringExpense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        recurringExpense = await _unitOfWork.RecurringExpenses.GetByPublicIdAsync(userId, recurringExpense.PublicId, cancellationToken)
            ?? recurringExpense;

        return _mapper.Map<RecurringExpenseDto>(recurringExpense);
    }

    public async Task<RecurringExpenseDto?> GetAsync(int userId, Guid recurringExpenseId, CancellationToken cancellationToken = default)
    {
        var recurringExpense = await _unitOfWork.RecurringExpenses.GetByPublicIdAsync(userId, recurringExpenseId, cancellationToken);
        return recurringExpense is null ? null : _mapper.Map<RecurringExpenseDto>(recurringExpense);
    }

    public async Task<IReadOnlyList<RecurringExpenseDto>> ListAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.RecurringExpenses.GetByUserAsync(userId, cancellationToken);
        return items.Select(_mapper.Map<RecurringExpenseDto>).ToList();
    }

    public async Task<RecurringExpenseDto> UpdateAsync(int userId, Guid recurringExpenseId, UpdateRecurringExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var recurringExpense = await _unitOfWork.RecurringExpenses.GetByPublicIdAsync(userId, recurringExpenseId, cancellationToken)
            ?? throw new NotFoundException("Recurring expense not found.");

        var categoryId = await ResolveCategoryIdAsync(userId, request.CategoryId, cancellationToken);

        recurringExpense.CategoryId = categoryId;
        recurringExpense.Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
        recurringExpense.Currency = request.Currency.Trim().ToUpperInvariant();
        recurringExpense.Description = request.Description?.Trim();
        recurringExpense.Frequency = request.Frequency;
        recurringExpense.StartDate = request.StartDate;
        recurringExpense.EndDate = request.EndDate;
        recurringExpense.IsActive = request.IsActive;
        recurringExpense.NextOccurrence = CalculateInitialNextOccurrence(request.StartDate, request.Frequency, _dateTimeProvider.Today);

        _unitOfWork.RecurringExpenses.Update(recurringExpense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        recurringExpense = await _unitOfWork.RecurringExpenses.GetByPublicIdAsync(userId, recurringExpense.PublicId, cancellationToken)
            ?? recurringExpense;

        return _mapper.Map<RecurringExpenseDto>(recurringExpense);
    }

    public async Task DeleteAsync(int userId, Guid recurringExpenseId, CancellationToken cancellationToken = default)
    {
        var recurringExpense = await _unitOfWork.RecurringExpenses.GetByPublicIdAsync(userId, recurringExpenseId, cancellationToken)
            ?? throw new NotFoundException("Recurring expense not found.");

        _unitOfWork.RecurringExpenses.Remove(recurringExpense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ProcessDueRecurringExpensesAsync(CancellationToken cancellationToken = default)
    {
        var today = _dateTimeProvider.Today;
        var dueExpenses = await _unitOfWork.RecurringExpenses.GetDueAsync(today, cancellationToken);

        var created = 0;
        var hasChanges = false;

        foreach (var recurring in dueExpenses)
        {
            if (!recurring.IsActive)
            {
                continue;
            }

            while (recurring.IsActive && recurring.NextOccurrence <= today)
            {
                if (recurring.EndDate.HasValue && recurring.NextOccurrence > recurring.EndDate.Value)
                {
                recurring.IsActive = false;
                hasChanges = true;
                    break;
                }

                var expense = new Expense
                {
                    UserId = recurring.UserId,
                    CategoryId = recurring.CategoryId,
                    Amount = recurring.Amount,
                    Currency = recurring.Currency,
                    Description = recurring.Description,
                    ExpenseDate = recurring.NextOccurrence,
                    IsRecurring = true,
                    RecurringExpenseId = recurring.Id
                };

                await _unitOfWork.Expenses.AddAsync(expense, cancellationToken);
                created++;
                hasChanges = true;

                recurring.NextOccurrence = CalculateNextOccurrence(recurring.NextOccurrence, recurring.Frequency);
            }

            if (recurring.EndDate.HasValue && recurring.NextOccurrence > recurring.EndDate.Value)
            {
                recurring.IsActive = false;
                hasChanges = true;
            }

            _unitOfWork.RecurringExpenses.Update(recurring);
        }

        if (hasChanges)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return created;
    }

    private async Task<int?> ResolveCategoryIdAsync(int userId, Guid? categoryId, CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue)
        {
            return null;
        }

        var category = await _unitOfWork.Categories.GetByPublicIdAsync(categoryId.Value, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (category.UserId != userId)
        {
            throw new UnauthorizedAppException("You are not allowed to use this category.");
        }

        return category.Id;
    }

    private static DateOnly CalculateInitialNextOccurrence(DateOnly startDate, RecurrenceFrequency frequency, DateOnly today)
    {
        var next = startDate;

        while (next < today)
        {
            next = CalculateNextOccurrence(next, frequency);
        }

        return next;
    }

    private static DateOnly CalculateNextOccurrence(DateOnly current, RecurrenceFrequency frequency)
    {
        return frequency switch
        {
            RecurrenceFrequency.Daily => current.AddDays(1),
            RecurrenceFrequency.Weekly => current.AddDays(7),
            RecurrenceFrequency.BiWeekly => current.AddDays(14),
            RecurrenceFrequency.Monthly => current.AddMonths(1),
            RecurrenceFrequency.Quarterly => current.AddMonths(3),
            RecurrenceFrequency.Yearly => current.AddYears(1),
            _ => current.AddDays(1)
        };
    }
}

