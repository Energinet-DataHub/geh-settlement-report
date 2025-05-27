using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;

public interface IMeasurementsReportService
{
    Task<IEnumerable<RequestedMeasurementsReportDto>> GetReportsAsync(Guid actorId);
}
