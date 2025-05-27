using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Persistence.MeasurementsReport;

public sealed class MeasurementsReportRepository : IMeasurementsReportRepository
{
    public Task AddOrUpdateAsync(Application.SettlementReports_v2.MeasurementsReport request)
    {
        throw new NotImplementedException();
    }

    public Task<Application.SettlementReports_v2.MeasurementsReport> GetAsync(string requestId)
    {
        // TODO BJM: Replace dummy implementation when story #784 is completed
        return Task.FromResult(new Application.SettlementReports_v2.MeasurementsReport(blobFileName: "foo"));
    }

    public Task<IEnumerable<Application.SettlementReports_v2.MeasurementsReport>> GetByActorIdAsync(Guid actorId)
    {
        throw new NotImplementedException();
    }

    public Task<Application.SettlementReports_v2.MeasurementsReport> GetByJobRunIdAsync(long jobRunId)
    {
        throw new NotImplementedException();
    }
}
