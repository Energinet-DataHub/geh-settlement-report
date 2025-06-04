using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.MeasurementsReport;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;

public sealed class MeasurementsReportFileRepository : IMeasurementsReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public MeasurementsReportFileRepository(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<Stream> DownloadAsync(string fileName)
    {
        var blobName = string.Join('/', "measurementsreports", fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenReadAsync().ConfigureAwait(false);
    }
}
