namespace Energinet.DataHub.Reports.Application.SettlementReports_v2;

public interface IMeasurementsReportRepository
{
    Task AddOrUpdateAsync(MeasurementsReport measurementsReport);

    Task<MeasurementsReport> GetAsync(string requestId);

    Task<IEnumerable<MeasurementsReport>> GetByActorIdAsync(Guid actorId);

    Task<MeasurementsReport> GetByJobRunIdAsync(long jobRunId);
}
