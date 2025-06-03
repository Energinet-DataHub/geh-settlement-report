using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Energinet.DataHub.Reports.Test.Core.Fixture.Database;

/// <summary>
/// An xUnit fixture for sharing a wholesale database for integration tests.
///
/// This class ensures the following:
///  * Integration test instances that uses the same fixture instance, uses the same database.
///  * The database is created similar to what we expect in a production environment (e.g. collation)
///  * Each fixture instance has an unique database instance (connection string).
/// </summary>
public sealed class WholesaleDatabaseFixture<TDatabaseContext> : IAsyncLifetime
    where TDatabaseContext : DbContext, new()
{
    public WholesaleDatabaseFixture()
    {
        DatabaseManager = new WholesaleDatabaseManager<TDatabaseContext>();
    }

    public WholesaleDatabaseManager<TDatabaseContext> DatabaseManager { get; }

    public Task InitializeAsync()
    {
        return DatabaseManager.CreateDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return DatabaseManager.DeleteDatabaseAsync();
    }
}
