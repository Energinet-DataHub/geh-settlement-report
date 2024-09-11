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

using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public sealed class GetSettlementReportsHandler : IGetSettlementReportsHandler
{
    private readonly ISettlementReportRepository _settlementReportRepository;
    private readonly IRemoveExpiredSettlementReports _removeExpiredSettlementReports;

    public GetSettlementReportsHandler(
        ISettlementReportRepository settlementReportRepository,
        IRemoveExpiredSettlementReports removeExpiredSettlementReports)
    {
        _settlementReportRepository = settlementReportRepository;
        _removeExpiredSettlementReports = removeExpiredSettlementReports;
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetAsync()
    {
        var settlementReports = (await _settlementReportRepository
                .GetAsync()
                .ConfigureAwait(false))
            .ToList();

        await _removeExpiredSettlementReports.RemoveExpiredAsync(settlementReports).ConfigureAwait(false);
        return settlementReports.Select(Map);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetAsync(Guid actorId)
    {
        var settlementReports = (await _settlementReportRepository
                .GetAsync(actorId)
                .ConfigureAwait(false))
            .ToList();

        await _removeExpiredSettlementReports.RemoveExpiredAsync(settlementReports).ConfigureAwait(false);
        return settlementReports.Select(Map);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetForJobsAsync()
    {
        var settlementReports = (await _settlementReportRepository
                .GetForJobsAsync()
                .ConfigureAwait(false))
            .ToList();

        await _removeExpiredSettlementReports.RemoveExpiredAsync(settlementReports).ConfigureAwait(false);
        return settlementReports.Select(Map);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetForJobsAsync(Guid actorId)
    {
        var settlementReports = (await _settlementReportRepository
                .GetForJobsAsync(actorId)
                .ConfigureAwait(false))
            .ToList();

        await _removeExpiredSettlementReports.RemoveExpiredAsync(settlementReports).ConfigureAwait(false);
        return settlementReports.Select(Map);
    }

    private static RequestedSettlementReportDto Map(SettlementReport report)
    {
        return new RequestedSettlementReportDto(
            report.RequestId is not null ? new SettlementReportRequestId(report.RequestId) : null,
            report.CalculationType,
            report.PeriodStart.ToDateTimeOffset(),
            report.PeriodEnd.ToDateTimeOffset(),
            report.Status,
            report.GridAreaCount,
            0,
            report.ActorId,
            report.ContainsBasisData,
            report.JobId is not null ? new JobRunId(report.JobId.Value) : null,
            report.CreatedDateTime.ToDateTimeOffset(),
            report.EndedDateTime?.ToDateTimeOffset());
    }
}
