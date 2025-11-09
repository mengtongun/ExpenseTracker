using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class RecurringExpenseRepository : IRecurringExpenseRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RecurringExpenseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RecurringExpense recurringExpense, CancellationToken cancellationToken = default)
    {
        await _dbContext.RecurringExpenses.AddAsync(recurringExpense, cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringExpense>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecurringExpenses
            .AsNoTracking()
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.NextOccurrence)
            .ToListAsync(cancellationToken);
    }

    public Task<RecurringExpense?> GetByIdAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.RecurringExpenses
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);
    }

    public Task<RecurringExpense?> GetByPublicIdAsync(int userId, Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.RecurringExpenses
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.PublicId == publicId && r.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringExpense>> GetDueAsync(DateOnly referenceDate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecurringExpenses
            .Where(r => r.IsActive && r.NextOccurrence <= referenceDate)
            .ToListAsync(cancellationToken);
    }

    public void Remove(RecurringExpense recurringExpense)
    {
        _dbContext.RecurringExpenses.Remove(recurringExpense);
    }

    public void Update(RecurringExpense recurringExpense)
    {
        _dbContext.RecurringExpenses.Update(recurringExpense);
    }
}

