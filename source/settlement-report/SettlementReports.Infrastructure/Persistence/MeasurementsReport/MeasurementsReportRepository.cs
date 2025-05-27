using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Persistence.MeasurementsReport;

public sealed class MeasurementsReportRepository : IMeasurementsReportRepository
{
    public Task<Application.SettlementReports_v2.MeasurementsReport> GetAsync(string requestId)
    {
        // TODO BJM: Replace dummy implementation when story #784 is completed
        return Task.FromResult(new Application.SettlementReports_v2.MeasurementsReport(blobFileName: "foo"));
    }
}
