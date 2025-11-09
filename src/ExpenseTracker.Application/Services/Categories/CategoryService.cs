using AutoMapper;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Contracts.Categories;
using ExpenseTracker.Domain.Entities;
using System;

namespace ExpenseTracker.Application.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoryDto> CreateAsync(int userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Categories.ExistsByNameAsync(userId, request.Name.Trim(), cancellationToken))
        {
            throw new ConflictException("Category name already exists.");
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            UserId = userId
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto?> GetAsync(int userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByPublicIdAsync(categoryId, cancellationToken);
        if (category is null || category.UserId != userId)
        {
            return null;
        }

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<IReadOnlyList<CategoryDto>> ListAsync(int userId, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetByUserAsync(userId, cancellationToken);
        return categories.Select(_mapper.Map<CategoryDto>).ToList();
    }

    public async Task<CategoryDto> UpdateAsync(int userId, Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByPublicIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (category.UserId != userId)
        {
            throw new UnauthorizedAppException("You are not allowed to update this category.");
        }

        var normalizedName = request.Name.Trim();
        if (await _unitOfWork.Categories.ExistsByNameAsync(userId, normalizedName, cancellationToken, category.Id))
        {
            throw new ConflictException("Category name already exists.");
        }

        category.Name = normalizedName;
        category.Description = request.Description?.Trim();
        _unitOfWork.Categories.Update(category);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(int userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByPublicIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (category.UserId != userId)
        {
            throw new UnauthorizedAppException("You are not allowed to delete this category.");
        }

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

