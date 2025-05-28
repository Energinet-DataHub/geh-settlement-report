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

using Energinet.DataHub.Reports.Application.Model;
using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Services;

public sealed class SettlementReportFileService : ISettlementReportFileService
{
    private readonly IReportFileRepository _reportFileRepository;
    private readonly ISettlementReportRepository _repository;

    public SettlementReportFileService(
        IReportFileRepository reportFileRepository,
        ISettlementReportRepository repository)
    {
        _reportFileRepository = reportFileRepository;
        _repository = repository;
    }

    public async Task<Stream> DownloadAsync(
        ReportRequestId requestId,
        Guid actorId,
        bool isMultitenancy)
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

        return await _reportFileRepository
            .DownloadAsync(ReportType.Settlement, report.BlobFileName)
            .ConfigureAwait(false);
    }
}
