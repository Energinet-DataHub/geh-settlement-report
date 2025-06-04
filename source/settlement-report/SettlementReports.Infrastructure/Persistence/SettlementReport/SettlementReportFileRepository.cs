using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Infrastructure.Extensions.Options;
using Energinet.DataHub.Reports.Interfaces.Models;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReport;

public sealed class SettlementReportFileRepository : ISettlementReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly IOptions<SettlementReportStorageOptions> _options;

    public SettlementReportFileRepository(BlobContainerClient blobContainerClient, IOptions<SettlementReportStorageOptions> options)
    {
        _blobContainerClient = blobContainerClient;
        _options = options;
    }

    public async Task<bool> DeleteAsync(ReportRequestId reportRequestId, string fileName)
    {
        var blobName = string.Join('/', _options.Value.DirectoryPath, reportRequestId.Id, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);
    }

    public async Task<Stream> DownloadAsync(string fileName)
    {
        var blobName = string.Join('/', _options.Value.DirectoryPath, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenReadAsync().ConfigureAwait(false);
    }
}
