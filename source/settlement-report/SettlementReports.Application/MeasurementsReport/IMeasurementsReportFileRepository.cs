using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport;

public interface IMeasurementsReportFileRepository
{
    Task DeleteAsync(ReportRequestId reportRequestId, string fileName);

    Task<Stream> DownloadAsync(string fileName);
}
