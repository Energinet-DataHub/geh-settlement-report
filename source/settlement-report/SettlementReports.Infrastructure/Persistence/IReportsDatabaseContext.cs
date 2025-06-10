using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence;

public interface IReportsDatabaseContext
{
    DbSet<Application.SettlementReports.SettlementReport> SettlementReports { get; }

    DbSet<Application.MeasurementsReport.MeasurementsReport> MeasurementsReports { get; }

    DbSet<GridAreaOwnerEntity> GridAreaOwners { get; }

    Task<int> SaveChangesAsync();
}
