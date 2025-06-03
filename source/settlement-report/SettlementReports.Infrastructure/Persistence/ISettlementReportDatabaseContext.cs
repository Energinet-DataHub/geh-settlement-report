using Energinet.DataHub.Reports.Application.SettlementReports;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence;

public interface ISettlementReportDatabaseContext
{
    DbSet<SettlementReport> SettlementReports { get; }

    DbSet<Application.MeasurementsReport.MeasurementsReport> MeasurementsReports { get; }

    DbSet<GridAreaOwnerEntity> GridAreaOwners { get; }

    Task<int> SaveChangesAsync();
}
