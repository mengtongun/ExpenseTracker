using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddAsync(category, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(int userId, string name, CancellationToken cancellationToken = default, int? excludeCategoryId = null)
    {
        var query = _dbContext.Categories
            .AsQueryable()
            .Where(c => c.UserId == userId && c.Name == name);

        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Category?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.FirstOrDefaultAsync(c => c.PublicId == publicId, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public void Remove(Category category)
    {
        _dbContext.Categories.Remove(category);
    }

    public void Update(Category category)
    {
        _dbContext.Categories.Update(category);
    }
}

