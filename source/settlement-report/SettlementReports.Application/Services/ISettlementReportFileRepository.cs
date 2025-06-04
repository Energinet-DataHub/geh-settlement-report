namespace Energinet.DataHub.Reports.Application.Services;

public interface ISettlementReportFileRepository
{
    Task<Stream> DownloadAsync(string fileName);
}
