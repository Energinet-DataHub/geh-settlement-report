using Energinet.DataHub.Reports.Abstractions.Model;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport;

public interface IMeasurementsReportFileRepository
{
    Task<bool> DeleteAsync(ReportRequestId reportRequestId, string fileName);

    Task<Stream> DownloadAsync(string fileName);
}
