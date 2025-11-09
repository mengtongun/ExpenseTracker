using ExpenseTracker.Domain.Enums;
using System;

namespace ExpenseTracker.Application.Contracts.RecurringExpenses;

public class CreateRecurringExpenseRequest
{
    public Guid? CategoryId { get; set; }
    public required decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

