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

using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Reports.Infrastructure.SettlementReports_v2;

public sealed class RemoveExpiredSettlementReports : IRemoveExpiredSettlementReports
{
    private readonly IClock _clock;
    private readonly ISettlementReportRepository _settlementReportRepository;
    private readonly ISettlementReportFileRepository _settlementReportFileRepository;
    private readonly IReportFileRepository _reportFileRepository;

    public RemoveExpiredSettlementReports(
        IClock clock,
        ISettlementReportRepository settlementReportRepository,
        ISettlementReportFileRepository settlementReportFileRepository,
        IReportFileRepository reportFileRepository)
    {
        _clock = clock;
        _settlementReportRepository = settlementReportRepository;
        _settlementReportFileRepository = settlementReportFileRepository;
        _reportFileRepository = reportFileRepository;
    }

    public async Task RemoveExpiredAsync(IList<Application.SettlementReports_v2.SettlementReport> settlementReports)
    {
        for (var i = 0; i < settlementReports.Count; i++)
        {
            var settlementReport = settlementReports[i];

            if (!IsExpired(settlementReport))
                continue;

            // Delete the blob file if it exists and the job id is null, because on the shared blob storage we use retention to clean-up
            if (settlementReport is { BlobFileName: not null, JobId: null })
            {
                await _settlementReportFileRepository
                    .DeleteAsync(
                        new ReportRequestId(settlementReport.RequestId),
                        settlementReport.BlobFileName)
                    .ConfigureAwait(false);
            }

            await _settlementReportRepository
                .DeleteAsync(settlementReport)
                .ConfigureAwait(false);

            settlementReports.RemoveAt(i--);
        }
    }

    private bool IsExpired(Application.SettlementReports_v2.SettlementReport settlementReport)
    {
        var cutOffPeriod = _clock
            .GetCurrentInstant()
            .Minus(TimeSpan.FromDays(7).ToDuration());

        return settlementReport.Status != ReportStatus.InProgress &&
               settlementReport.CreatedDateTime <= cutOffPeriod;
    }
}
