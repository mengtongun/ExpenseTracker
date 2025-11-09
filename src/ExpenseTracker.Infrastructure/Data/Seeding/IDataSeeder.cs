using System.Threading;
using System.Threading.Tasks;

namespace ExpenseTracker.Infrastructure.Data.Seeding;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
