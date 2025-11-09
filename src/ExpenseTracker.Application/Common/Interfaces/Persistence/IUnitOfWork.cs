using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IExpenseRepository Expenses { get; }
    IRecurringExpenseRepository RecurringExpenses { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IReportRepository Reports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

