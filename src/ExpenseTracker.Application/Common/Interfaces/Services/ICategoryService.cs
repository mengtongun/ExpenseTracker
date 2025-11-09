using ExpenseTracker.Application.Contracts.Categories;
using System;

namespace ExpenseTracker.Application.Common.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(int userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetAsync(int userId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> ListAsync(int userId, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(int userId, Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int userId, Guid categoryId, CancellationToken cancellationToken = default);
}

