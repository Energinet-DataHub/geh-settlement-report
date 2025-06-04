using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.Services;

namespace Energinet.DataHub.Reports.Infrastructure.Services;

public sealed class SettlementReportFileRepository : ISettlementReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public SettlementReportFileRepository(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<Stream> DownloadAsync(string fileName)
    {
        var blobName = string.Join('/', "settlementreports", fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenReadAsync().ConfigureAwait(false);
    }
}
