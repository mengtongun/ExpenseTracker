using ExpenseTracker.Application.Common.Interfaces.Services;

namespace ExpenseTracker.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly Today => DateOnly.FromDateTime(UtcNow);
}

