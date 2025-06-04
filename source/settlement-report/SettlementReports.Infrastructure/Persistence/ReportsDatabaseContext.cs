using Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;
using Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReport;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence;

public class ReportsDatabaseContext : DbContext, IReportsDatabaseContext
{
    private const string Schema = "settlementreports";

    public ReportsDatabaseContext(DbContextOptions<ReportsDatabaseContext> options)
        : base(options)
    {
    }

    public ReportsDatabaseContext() { }

    public DbSet<Application.SettlementReports.SettlementReport> SettlementReports { get; init; } = null!;

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
