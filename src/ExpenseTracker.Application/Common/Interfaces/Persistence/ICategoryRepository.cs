using ExpenseTracker.Domain.Entities;
using System;

namespace ExpenseTracker.Application.Common.Interfaces.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Category?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(int userId, string name, CancellationToken cancellationToken = default, int? excludeCategoryId = null);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    void Update(Category category);
    void Remove(Category category);
}

