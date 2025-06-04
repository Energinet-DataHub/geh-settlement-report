using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

namespace Energinet.DataHub.Reports.Interfaces;

public interface IGetSettlementReportsHandler
{
    Task<IEnumerable<RequestedSettlementReportDto>> GetForJobsAsync();

    Task<IEnumerable<RequestedSettlementReportDto>> GetForJobsAsync(Guid actorId);
}
