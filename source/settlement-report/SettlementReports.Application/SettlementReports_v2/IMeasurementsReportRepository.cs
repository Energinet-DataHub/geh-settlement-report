namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public interface IMeasurementsReportRepository
{
    Task AddOrUpdateAsync(MeasurementsReport request);

    Task<MeasurementsReport> GetAsync(string requestId);

    Task<IEnumerable<MeasurementsReport>> GetByActorIdAsync(Guid actorId);

    Task<MeasurementsReport> GetByJobRunIdAsync(long jobRunId);
}
