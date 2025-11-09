using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.Contracts.Expenses;
using System;

namespace ExpenseTracker.Application.Common.Interfaces.Services;

public interface IExpenseService
{
    Task<ExpenseDto> CreateAsync(int userId, CreateExpenseRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseDto?> GetAsync(int userId, Guid expenseId, CancellationToken cancellationToken = default);
    Task<PagedResult<ExpenseDto>> SearchAsync(int userId, ExpenseQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ExpenseDto> UpdateAsync(int userId, Guid expenseId, UpdateExpenseRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int userId, Guid expenseId, CancellationToken cancellationToken = default);
}

