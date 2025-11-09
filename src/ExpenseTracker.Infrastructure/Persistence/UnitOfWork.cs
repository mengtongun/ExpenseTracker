using ExpenseTracker.Application.Common.Interfaces.Persistence;
using ExpenseTracker.Infrastructure.Data;

namespace ExpenseTracker.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(
        ApplicationDbContext dbContext,
        IUserRepository users,
        ICategoryRepository categories,
        IExpenseRepository expenses,
        IRecurringExpenseRepository recurringExpenses,
        IRefreshTokenRepository refreshTokens,
        IReportRepository reports)
    {
        _dbContext = dbContext;
        Users = users;
        Categories = categories;
        Expenses = expenses;
        RecurringExpenses = recurringExpenses;
        RefreshTokens = refreshTokens;
        Reports = reports;
    }

    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }
    public IExpenseRepository Expenses { get; }
    public IRecurringExpenseRepository RecurringExpenses { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IReportRepository Reports { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

