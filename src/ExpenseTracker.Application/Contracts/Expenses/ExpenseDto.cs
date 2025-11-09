using System;

namespace ExpenseTracker.Application.Contracts.Expenses;

public class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public bool IsRecurring { get; set; }
}

