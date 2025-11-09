using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Expense : AuditableEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public bool IsRecurring { get; set; }

    public int? RecurringExpenseId { get; set; }
    public RecurringExpense? RecurringExpense { get; set; }
}

