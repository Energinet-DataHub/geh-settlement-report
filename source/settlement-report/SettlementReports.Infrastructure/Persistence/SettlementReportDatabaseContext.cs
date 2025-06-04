using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;
using Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReportRequest;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence;

public class SettlementReportDatabaseContext : DbContext, ISettlementReportDatabaseContext
{
    private const string Schema = "settlementreports";

    public SettlementReportDatabaseContext(DbContextOptions<SettlementReportDatabaseContext> options)
        : base(options)
    {
    }

    public SettlementReportDatabaseContext() { }

    public DbSet<SettlementReport> SettlementReports { get; init; } = null!;

    public DbSet<Application.MeasurementsReport.MeasurementsReport> MeasurementsReports { get; init; } = null!;

    public DbSet<GridAreaOwnerEntity> GridAreaOwners { get; init; } = null!;

    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfiguration(new SettlementReportEntityConfiguration());
        modelBuilder.ApplyConfiguration(new GridAreaOwnerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new MeasurementsReportEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
