using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.MeasurementsReport;
using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;

public sealed class MeasurementsReportFileRepository : IMeasurementsReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public MeasurementsReportFileRepository(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<bool> DeleteAsync(ReportRequestId reportRequestId, string fileName)
    {
        var blobName = string.Join('/', "measurementsreports", reportRequestId.Id, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);
    }

    public async Task<Stream> DownloadAsync(string fileName)
    {
        var blobName = string.Join('/', "measurementsreports", fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenReadAsync().ConfigureAwait(false);
    }
}
