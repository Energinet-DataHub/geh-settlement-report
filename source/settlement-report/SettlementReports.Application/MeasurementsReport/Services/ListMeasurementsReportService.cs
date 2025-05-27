using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;

public sealed class ListMeasurementsReportService : IListMeasurementsReportService
{
    public Task<IEnumerable<RequestedMeasurementsReportDto>> GetAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<RequestedMeasurementsReportDto>> GetAsync(Guid actorId)
    {
        throw new NotImplementedException();
    }
}
