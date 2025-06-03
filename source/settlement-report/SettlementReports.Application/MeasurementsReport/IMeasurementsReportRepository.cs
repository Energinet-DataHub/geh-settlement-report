namespace Energinet.DataHub.Reports.Application.MeasurementsReport;

public interface IMeasurementsReportRepository
{
    Task AddOrUpdateAsync(MeasurementsReport measurementsReport);

    Task<MeasurementsReport> GetByRequestIdAsync(string requestId);

    Task<IEnumerable<MeasurementsReport>> GetByActorIdAsync(Guid actorId);

    Task<MeasurementsReport> GetByJobRunIdAsync(long jobRunId);
}
