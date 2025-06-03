using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.Model;
using Energinet.DataHub.Reports.Application.SettlementReports_v2;

namespace Energinet.DataHub.Reports.Infrastructure.SettlementReports_v2;

public sealed class ReportFileRepository : IReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public ReportFileRepository(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<Stream> DownloadAsync(ReportType reportType, string fileName)
    {
        var blobName = GetBlobName(reportType, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenReadAsync().ConfigureAwait(false);
    }

    private static string GetBlobName(ReportType reportType, string fileName) => reportType switch
    {
        ReportType.Settlement => string.Join('/', "settlementreports", fileName),
        ReportType.Measurements => string.Join('/', "measurementsreports", fileName),
        _ => throw new NotSupportedException($"Report type {reportType} is not supported."),
    };
}
