// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Azure.Storage.Blobs;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportJobsFileBlobStorage : ISettlementReportJobsFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public SettlementReportJobsFileBlobStorage(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task DownloadAsync(JobRunId jobRunId, string fileName, Stream downloadStream)
    {
        var blobName = GetBlobName(jobRunId, fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        await blobClient.DownloadToAsync(downloadStream).ConfigureAwait(false);
    }

    public Task DeleteAsync(JobRunId reportRequestId, string fileName)
    {
        return Task.CompletedTask;
    }

    private static string GetBlobName(JobRunId jobRunId, string fileName)
    {
        return string.Join('/', "settlement-reports", "reports", jobRunId.Id, fileName);
    }
}
