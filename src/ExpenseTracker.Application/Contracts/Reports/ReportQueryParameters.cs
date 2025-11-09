namespace ExpenseTracker.Application.Contracts.Reports;

public class ReportQueryParameters
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string GroupBy { get; set; } = "category";
}

