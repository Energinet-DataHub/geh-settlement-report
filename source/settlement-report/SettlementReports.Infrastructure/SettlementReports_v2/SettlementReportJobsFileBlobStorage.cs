﻿// Copyright 2020 Energinet DataHub A/S
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

namespace Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2;

public sealed class SettlementReportJobsFileBlobStorage : ISettlementReportJobsFileRepository
{
    private readonly BlobContainerClient _blobContainerClient;

    public SettlementReportJobsFileBlobStorage(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task<Stream> DownloadAsync(string fileName)
    {
        var blobName = GetBlobName(fileName);
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        return await blobClient.OpenReadAsync().ConfigureAwait(false);
    }

    private static string GetBlobName(string fileName)
    {
        return string.Join('/', "settlementreports", fileName);
    }
}
