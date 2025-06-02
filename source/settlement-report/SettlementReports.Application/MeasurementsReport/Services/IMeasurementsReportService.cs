using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public interface IMeasurementsReportService
{
    Task<IEnumerable<RequestedMeasurementsReportDto>> GetReportsAsync(Guid actorId);

    Task CancelAsync(ReportRequestId reportRequestId, Guid userId);
}
