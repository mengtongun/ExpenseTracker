using System;

namespace ExpenseTracker.Application.Contracts.Expenses;

public class CreateExpenseRequest
{
    public Guid? CategoryId { get; set; }
    public required decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public bool IsRecurring { get; set; }
}

