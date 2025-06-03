using Energinet.DataHub.Reports.Application.Model;

namespace Energinet.DataHub.Reports.Application.Services;

public interface IReportFileRepository
{
    Task<Stream> DownloadAsync(ReportType reportType, string blobFileName);
}
