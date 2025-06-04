using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Infrastructure.Services;

public sealed class SettlementReportFileRepository : ISettlementReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public SettlementReportFileRepository(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<bool> DeleteAsync(ReportRequestId reportRequestId, string fileName)
    {
        var blobName = string.Join('/', "settlementreports", reportRequestId.Id, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);
    }

    public async Task<Stream> DownloadAsync(string fileName)
    {
        var blobName = string.Join('/', "settlementreports", fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenReadAsync().ConfigureAwait(false);
    }
}
