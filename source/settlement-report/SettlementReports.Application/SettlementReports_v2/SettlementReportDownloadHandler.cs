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

using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public sealed class SettlementReportDownloadHandler : ISettlementReportDownloadHandler
{
    private readonly ISettlementReportFileRepository _fileRepository;
    private readonly ISettlementReportRepository _repository;

    public SettlementReportDownloadHandler(
        ISettlementReportFileRepository fileRepository,
        ISettlementReportRepository repository)
    {
        _fileRepository = fileRepository;
        _repository = repository;
    }

    public async Task DownloadReportAsync(ReportRequestId requestId, Func<Stream> outputStreamProvider, Guid actorId, bool isMultitenancy)
    {
        var report = await _repository
            .GetAsync(requestId.Id)
            .ConfigureAwait(false) ?? throw new InvalidOperationException("Report not found.");

        if (!isMultitenancy && (report.ActorId != actorId || report.IsHiddenFromActor))
        {
            throw new InvalidOperationException("User does not have access to the report.");
        }

        if (string.IsNullOrEmpty(report.BlobFileName))
            throw new InvalidOperationException("Report does not have a Blob file name.");

        await _fileRepository
            .DownloadAsync(requestId, report.BlobFileName, outputStreamProvider())
            .ConfigureAwait(false);
    }
}
