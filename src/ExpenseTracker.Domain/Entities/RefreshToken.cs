using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class RefreshToken : AuditableEntity
{
    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsActive => RevokedAt is null && DateTime.UtcNow <= ExpiresAt;
}

