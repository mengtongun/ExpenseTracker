using System;

namespace ExpenseTracker.Application.Contracts.Categories;

public class CategoryDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

