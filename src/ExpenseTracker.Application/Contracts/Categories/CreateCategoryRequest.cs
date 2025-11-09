namespace ExpenseTracker.Application.Contracts.Categories;

public class CreateCategoryRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

