using ExpenseTracker.Application.Contracts.RecurringExpenses;
using System;

namespace ExpenseTracker.Application.Common.Interfaces.Services;

public interface IRecurringExpenseService
{
    Task<RecurringExpenseDto> CreateAsync(int userId, CreateRecurringExpenseRequest request, CancellationToken cancellationToken = default);
    Task<RecurringExpenseDto?> GetAsync(int userId, Guid recurringExpenseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringExpenseDto>> ListAsync(int userId, CancellationToken cancellationToken = default);
    Task<RecurringExpenseDto> UpdateAsync(int userId, Guid recurringExpenseId, UpdateRecurringExpenseRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int userId, Guid recurringExpenseId, CancellationToken cancellationToken = default);
    Task<int> ProcessDueRecurringExpensesAsync(CancellationToken cancellationToken = default);
}

