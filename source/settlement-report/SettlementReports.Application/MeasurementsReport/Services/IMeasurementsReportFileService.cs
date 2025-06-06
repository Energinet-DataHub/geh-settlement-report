using Energinet.DataHub.Reports.Abstractions.Model;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public interface IMeasurementsReportFileService
{
    Task<Stream> DownloadAsync(ReportRequestId requestId);
}
