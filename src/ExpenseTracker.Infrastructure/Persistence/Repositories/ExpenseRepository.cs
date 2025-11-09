using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ExpenseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        await _dbContext.Expenses.AddAsync(expense, cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> GetByUserAsync(int userId, DateOnly? startDate = null, DateOnly? endDate = null, int? categoryId = null, CancellationToken cancellationToken = default)
    {
        var items = await BuildQuery(userId, startDate, endDate, categoryId)
            .AsNoTracking()
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.Id)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<(IReadOnlyList<Expense> Items, int TotalCount)> SearchAsync(int userId, DateOnly? startDate, DateOnly? endDate, int? categoryId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(userId, startDate, endDate, categoryId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.Id)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Max(take, 0))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Expense?> GetByIdAsync(int userId, int expenseId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == expenseId, cancellationToken);
    }

    public Task<Expense?> GetByPublicIdAsync(int userId, Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.PublicId == publicId, cancellationToken);
    }

    public void Remove(Expense expense) => _dbContext.Expenses.Remove(expense);

    public void Update(Expense expense) => _dbContext.Expenses.Update(expense);

    private IQueryable<Expense> BuildQuery(int userId, DateOnly? startDate, DateOnly? endDate, int? categoryId)
    {
        var query = _dbContext.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= endDate.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == categoryId.Value);
        }

        return query;
    }
}

