using System;

namespace ExpenseTracker.Application.Contracts.Reports;

public class ReportDto
{
    public Guid Id { get; set; }
    public required string ReportType { get; set; }
    public required string Parameters { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}

