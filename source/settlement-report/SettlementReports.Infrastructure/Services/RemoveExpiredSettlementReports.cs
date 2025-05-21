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

using Energinet.DataHub.SettlementReport.Application.Services;
using Energinet.DataHub.SettlementReport.Application.Services.SettlementReports;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Services;

public sealed class RemoveExpiredSettlementReports : IRemoveExpiredSettlementReports
{
    private readonly IClock _clock;
    private readonly ISettlementReportRepository _settlementReportRepository;

    public RemoveExpiredSettlementReports(
        IClock clock,
        ISettlementReportRepository settlementReportRepository,
        ISettlementReportJobsFileRepository settlementReportJobFileRepository)
    {
        _clock = clock;
        _settlementReportRepository = settlementReportRepository;
    }

    public async Task RemoveExpiredAsync(IList<Application.Model.SettlementReport> settlementReports)
    {
        for (var i = 0; i < settlementReports.Count; i++)
        {
            var settlementReport = settlementReports[i];

            if (!IsExpired(settlementReport))
                continue;

            await _settlementReportRepository
                .DeleteAsync(settlementReport)
                .ConfigureAwait(false);

            settlementReports.RemoveAt(i--);
        }
    }

    private bool IsExpired(Application.Model.SettlementReport settlementReport)
    {
        var cutOffPeriod = _clock
            .GetCurrentInstant()
            .Minus(TimeSpan.FromDays(7).ToDuration());

        return settlementReport.Status != SettlementReportStatus.InProgress &&
               settlementReport.CreatedDateTime <= cutOffPeriod;
    }
}
