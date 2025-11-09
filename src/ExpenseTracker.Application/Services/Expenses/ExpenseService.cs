using AutoMapper;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.Contracts.Expenses;
using ExpenseTracker.Domain.Entities;
using System;

namespace ExpenseTracker.Application.Services.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExpenseDto> CreateAsync(int userId, CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var categoryId = await ResolveCategoryIdAsync(userId, request.CategoryId, cancellationToken);

        var expense = new Expense
        {
            UserId = userId,
            CategoryId = categoryId,
            Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Description = request.Description?.Trim(),
            ExpenseDate = request.ExpenseDate,
            IsRecurring = request.IsRecurring
        };

        await _unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        expense = await _unitOfWork.Expenses.GetByPublicIdAsync(userId, expense.PublicId, cancellationToken)
            ?? expense;

        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<ExpenseDto?> GetAsync(int userId, Guid expenseId, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByPublicIdAsync(userId, expenseId, cancellationToken);
        return expense is null ? null : _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<PagedResult<ExpenseDto>> SearchAsync(int userId, ExpenseQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(parameters.PageNumber, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 200);
        var skip = (pageNumber - 1) * pageSize;

        var categoryId = await ResolveCategoryIdAsync(userId, parameters.CategoryId, cancellationToken);

        var (items, totalCount) = await _unitOfWork.Expenses.SearchAsync(
            userId,
            parameters.StartDate,
            parameters.EndDate,
            categoryId,
            skip,
            pageSize,
            cancellationToken);

        var dtos = items.Select(_mapper.Map<ExpenseDto>).ToList();

        return new PagedResult<ExpenseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ExpenseDto> UpdateAsync(int userId, Guid expenseId, UpdateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByPublicIdAsync(userId, expenseId, cancellationToken)
            ?? throw new NotFoundException("Expense not found.");

        var categoryId = await ResolveCategoryIdAsync(userId, request.CategoryId, cancellationToken);

        expense.CategoryId = categoryId;
        expense.Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
        expense.Currency = request.Currency.Trim().ToUpperInvariant();
        expense.Description = request.Description?.Trim();
        expense.ExpenseDate = request.ExpenseDate;
        expense.IsRecurring = request.IsRecurring;

        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        expense = await _unitOfWork.Expenses.GetByPublicIdAsync(userId, expense.PublicId, cancellationToken)
            ?? expense;

        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task DeleteAsync(int userId, Guid expenseId, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByPublicIdAsync(userId, expenseId, cancellationToken)
            ?? throw new NotFoundException("Expense not found.");

        _unitOfWork.Expenses.Remove(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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

}

