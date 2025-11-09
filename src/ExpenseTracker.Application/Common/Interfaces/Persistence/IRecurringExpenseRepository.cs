using ExpenseTracker.Domain.Entities;
using System;

namespace ExpenseTracker.Application.Common.Interfaces.Persistence;

public interface IRecurringExpenseRepository
{
    Task<RecurringExpense?> GetByIdAsync(int userId, int id, CancellationToken cancellationToken = default);
    Task<RecurringExpense?> GetByPublicIdAsync(int userId, Guid publicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringExpense>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringExpense>> GetDueAsync(DateOnly referenceDate, CancellationToken cancellationToken = default);
    Task AddAsync(RecurringExpense recurringExpense, CancellationToken cancellationToken = default);
    void Update(RecurringExpense recurringExpense);
    void Remove(RecurringExpense recurringExpense);
}

