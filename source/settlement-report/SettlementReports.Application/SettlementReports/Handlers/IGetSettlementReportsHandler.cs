using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Handlers;

public interface IGetSettlementReportsHandler
{
    Task<IEnumerable<RequestedSettlementReportDto>> GetForJobsAsync();

    Task<IEnumerable<RequestedSettlementReportDto>> GetForJobsAsync(Guid actorId);
}
