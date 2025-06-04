using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public interface IMeasurementsReportService
{
    Task<IEnumerable<RequestedMeasurementsReportDto>> GetReportsAsync(Guid actorId);

    Task CancelAsync(ReportRequestId reportRequestId, Guid userId);
}
