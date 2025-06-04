using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using Energinet.DataHub.SettlementReport.DatabaseMigration;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Test.Core.Fixture.Database;

public class WholesaleDatabaseManager<TDatabaseContext> : SqlServerDatabaseManager<TDatabaseContext>
where TDatabaseContext : DbContext, new()
{
    public WholesaleDatabaseManager()
        : base("Wholesale")
    {
    }

    /// <inheritdoc/>
    public override TDatabaseContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDatabaseContext>()
            .UseSqlServer(ConnectionString, options =>
            {
                options.UseNodaTime();
                options.EnableRetryOnFailure();
            });

        return (TDatabaseContext)Activator.CreateInstance(typeof(TDatabaseContext), optionsBuilder.Options)!;
    }

    /// <summary>
    /// Creates the database schema using DbUp instead of a database context.
    /// </summary>
    protected override Task<bool> CreateDatabaseSchemaAsync(TDatabaseContext context)
    {
        return Task.FromResult(CreateDatabaseSchema(context));
    }

    /// <summary>
    /// Creates the database schema using DbUp instead of a database context.
    /// </summary>
    protected override bool CreateDatabaseSchema(TDatabaseContext context)
    {
        var result = Upgrader.DatabaseUpgrade(ConnectionString);
        if (!result.Successful)
            throw new Exception("Database migration failed", result.Error);

        return true;
    }
}
