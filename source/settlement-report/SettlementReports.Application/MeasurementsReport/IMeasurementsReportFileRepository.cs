namespace Energinet.DataHub.Reports.Application.MeasurementsReport;

public interface IMeasurementsReportFileRepository
{
    Task<Stream> DownloadAsync(string fileName);
}
