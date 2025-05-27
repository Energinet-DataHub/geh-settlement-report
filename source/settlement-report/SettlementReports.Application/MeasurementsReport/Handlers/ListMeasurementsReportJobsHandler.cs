using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;

public sealed class ListMeasurementsReportJobsHandler : IListMeasurementsReportJobsHandler
{
    public Task<IEnumerable<RequestedMeasurementsReportDto>> HandleAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<RequestedMeasurementsReportDto>> HandleAsync(Guid actorId)
    {
        throw new NotImplementedException();
    }
}
