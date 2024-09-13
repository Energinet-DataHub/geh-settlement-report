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

using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using NodaTime;

namespace Energinet.DataHub.SettlementReport.Application.Handlers;

public sealed class ListSettlementReportJobsHandler : IListSettlementReportJobsHandler
{
    private readonly IDatabricksJobsHelper _jobHelper;
    private readonly IGetSettlementReportsHandler _getSettlementReportsHandler;
    private readonly ISettlementReportRepository _repository;
    private readonly IClock _clock;

    public ListSettlementReportJobsHandler(
        IDatabricksJobsHelper jobHelper,
        IGetSettlementReportsHandler getSettlementReportsHandler,
        ISettlementReportRepository repository,
        IClock clock)
    {
        _jobHelper = jobHelper;
        _getSettlementReportsHandler = getSettlementReportsHandler;
        _repository = repository;
        _clock = clock;
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync()
    {
        var settlementReports = await _getSettlementReportsHandler.GetForJobsAsync().ConfigureAwait(false);
        return await GetSettlementReportsAsync(settlementReports).ConfigureAwait(false);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync(Guid actorId)
    {
        var settlementReports = await _getSettlementReportsHandler.GetForJobsAsync(actorId).ConfigureAwait(false);
        return await GetSettlementReportsAsync(settlementReports).ConfigureAwait(false);
    }

    private async Task<IEnumerable<RequestedSettlementReportDto>> GetSettlementReportsAsync(IEnumerable<RequestedSettlementReportDto> settlementReports)
    {
        var results = new List<RequestedSettlementReportDto>();
        foreach (var settlementReportDto in settlementReports)
        {
            if (settlementReportDto.Status != SettlementReportStatus.Completed)
            {
                var jobStatus = await _jobHelper.GetSettlementReportsJobStatusAsync(settlementReportDto.JobId!.Id)
                    .ConfigureAwait(false);
                switch (jobStatus)
                {
                    case JobRunStatus.Completed:
                        await MarkAsCompletedAsync(settlementReportDto).ConfigureAwait(false);
                        break;
                    case JobRunStatus.Canceled or JobRunStatus.Failed:
                        await MarkAsFailedAsync(settlementReportDto).ConfigureAwait(false);
                        break;
                }

                results.Add(settlementReportDto with { Status = MapFromJobStatus(jobStatus) });
            }
            else
            {
                results.Add(settlementReportDto);
            }
        }

        return results;
    }

    private async Task MarkAsCompletedAsync(RequestedSettlementReportDto settlementReportDto)
    {
        ArgumentNullException.ThrowIfNull(settlementReportDto);
        ArgumentNullException.ThrowIfNull(settlementReportDto.JobId);

        var request = await _repository
            .GetAsync(settlementReportDto.JobId.Id)
            .ConfigureAwait(false);

        request.MarkAsCompleted(_clock, settlementReportDto.RequestId);

        await _repository
            .AddOrUpdateAsync(request)
            .ConfigureAwait(false);
    }

    private async Task MarkAsFailedAsync(RequestedSettlementReportDto settlementReportDto)
    {
        ArgumentNullException.ThrowIfNull(settlementReportDto);
        ArgumentNullException.ThrowIfNull(settlementReportDto.JobId);

        var request = await _repository
            .GetAsync(settlementReportDto.JobId.Id)
            .ConfigureAwait(false);

        request.MarkAsFailed();

        await _repository
            .AddOrUpdateAsync(request)
            .ConfigureAwait(false);
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
