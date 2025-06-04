using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Infrastructure.Services;

public sealed class SettlementReportFileBlobStorage : ISettlementReportFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public SettlementReportFileBlobStorage(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public Task DeleteAsync(ReportRequestId reportRequestId, string fileName)
    {
        var blobName = GetBlobName(reportRequestId, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return blobClient.DeleteIfExistsAsync();
    }

    private static string GetBlobName(ReportRequestId reportRequestId, string fileName)
    {
        return string.Join('/', "settlement-reports", reportRequestId.Id, fileName);
    }
}
