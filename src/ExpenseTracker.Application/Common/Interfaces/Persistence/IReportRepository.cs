using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Common.Interfaces.Persistence;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Report>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
}

