using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Category : AuditableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<RecurringExpense> RecurringExpenses { get; set; } = new List<RecurringExpense>();
}

