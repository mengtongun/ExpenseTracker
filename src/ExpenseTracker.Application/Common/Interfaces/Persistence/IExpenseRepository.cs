using ExpenseTracker.Domain.Entities;
using System;

namespace ExpenseTracker.Application.Common.Interfaces.Persistence;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(int userId, int expenseId, CancellationToken cancellationToken = default);
    Task<Expense?> GetByPublicIdAsync(int userId, Guid publicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetByUserAsync(int userId, DateOnly? startDate = null, DateOnly? endDate = null, int? categoryId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Expense> Items, int TotalCount)> SearchAsync(int userId, DateOnly? startDate, DateOnly? endDate, int? categoryId, int skip, int take, CancellationToken cancellationToken = default);
    Task AddAsync(Expense expense, CancellationToken cancellationToken = default);
    void Update(Expense expense);
    void Remove(Expense expense);
}

