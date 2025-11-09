using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Report : AuditableEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public required string ReportType { get; set; }
    public string Parameters { get; set; } = "{}";
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public byte[]? Content { get; set; }
}

