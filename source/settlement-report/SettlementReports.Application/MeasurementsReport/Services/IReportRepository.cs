using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;

public interface IReportRepository<T>
    where T : Report
{
    Task AddOrUpdateAsync(T report);

    Task<IEnumerable<T>> GetByActorIdAsync(Guid actorId);
}
