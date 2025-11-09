using ExpenseTracker.Domain.Enums;
using System;

namespace ExpenseTracker.Application.Contracts.RecurringExpenses;

public class RecurringExpenseDto
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly NextOccurrence { get; set; }
    public bool IsActive { get; set; }
}

