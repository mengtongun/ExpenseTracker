namespace ExpenseTracker.Application.Contracts.Reports;

public class ReportExportRequest : ReportQueryParameters
{
    public ReportExportFormat Format { get; set; } = ReportExportFormat.Csv;
}

