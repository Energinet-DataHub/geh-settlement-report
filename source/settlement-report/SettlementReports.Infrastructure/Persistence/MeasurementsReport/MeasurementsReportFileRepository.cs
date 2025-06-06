﻿using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Application.MeasurementsReport;
using Energinet.DataHub.Reports.Infrastructure.Extensions.Options;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;

public sealed class MeasurementsReportFileRepository : IMeasurementsReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly IOptions<MeasurementsReportStorageOptions> _options;

    public MeasurementsReportFileRepository(BlobContainerClient blobContainerClient, IOptions<MeasurementsReportStorageOptions> options)
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
        if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Invalid measurements report file name.", nameof(fileName));

        var blobName = string.Join('/', _options.Value.DirectoryPath, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenReadAsync().ConfigureAwait(false);
    }
}
