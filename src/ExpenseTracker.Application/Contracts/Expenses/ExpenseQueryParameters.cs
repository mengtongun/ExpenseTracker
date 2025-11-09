using System;

namespace ExpenseTracker.Application.Contracts.Expenses;

public class ExpenseQueryParameters
{
    public Guid? CategoryId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

