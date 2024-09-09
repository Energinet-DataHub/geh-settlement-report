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

using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.Handlers;

public sealed class ListSettlementReportJobsHandler : IListSettlementReportJobsHandler
{
    private readonly IDatabricksJobsHelper _jobHelper;
    private readonly IGetSettlementReportsHandler _getSettlementReportsHandler;

    public ListSettlementReportJobsHandler(
        IDatabricksJobsHelper jobHelper,
        IGetSettlementReportsHandler getSettlementReportsHandler)
    {
        _jobHelper = jobHelper;
        _getSettlementReportsHandler = getSettlementReportsHandler;
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync()
    {
        var settlementReports = (await _getSettlementReportsHandler
            .GetAsync()
            .ConfigureAwait(false))
            .Where(x => x.JobId is not null).ToList();

        var results = new List<RequestedSettlementReportDto>();
        foreach (var settlementReportDto in settlementReports)
        {
            var jobStatus = await _jobHelper.GetSettlementReportsJobStatusAsync(settlementReportDto.JobId!.Id).ConfigureAwait(false);
            results.Add(settlementReportDto with { Status = MapFromJobStatus(jobStatus) });
        }

        return results;
    }

    private SettlementReportStatus MapFromJobStatus(JobRunStatus status)
    {
        return status switch
        {
            JobRunStatus.Running => SettlementReportStatus.InProgress,
            JobRunStatus.Queued => SettlementReportStatus.InProgress,
            JobRunStatus.Completed => SettlementReportStatus.Completed,
            JobRunStatus.Canceled => SettlementReportStatus.Failed,
            JobRunStatus.Failed => SettlementReportStatus.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}
