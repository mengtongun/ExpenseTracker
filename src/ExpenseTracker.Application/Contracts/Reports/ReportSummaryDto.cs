namespace ExpenseTracker.Application.Contracts.Reports;

public class ReportSummaryDto
{
    public decimal Total { get; set; }
    public Dictionary<string, decimal> ByCategory { get; set; } = new();
    public Dictionary<string, decimal> ByMonth { get; set; } = new();
    public Dictionary<string, decimal>? ByCurrency { get; set; }
}

