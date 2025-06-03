using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public interface IMeasurementsReportFileService
{
    Task<Stream> DownloadAsync(ReportRequestId requestId);
}
