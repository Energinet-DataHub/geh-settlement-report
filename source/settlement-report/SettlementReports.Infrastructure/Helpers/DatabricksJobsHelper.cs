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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.SettlementReport.Application.Handlers;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Helpers;

public class DatabricksJobsHelper : IDatabricksJobsHelper
{
    private readonly IJobsApiClient _jobsApiClient;

    public DatabricksJobsHelper(IJobsApiClient jobsApiClient)
    {
        _jobsApiClient = jobsApiClient;
    }

    public async Task<JobRunId> RunSettlementReportsJobAsync(SettlementReportRequestDto request)
    {
        var job = await GetSettlementReportsJobAsync(GetJobName(request.Filter.CalculationType)).ConfigureAwait(false);
        return new JobRunId(await _jobsApiClient.Jobs.RunNow(job.JobId, CreateParameters(request)).ConfigureAwait(false));
    }

    public async Task<JobRunStatus> GetSettlementReportsJobStatusAsync(long runId)
    {
        var jobRun = await _jobsApiClient.Jobs.RunsGet(runId, false).ConfigureAwait(false);
        return ConvertJobStatus(jobRun.Item1);
    }

    private string GetJobName(CalculationType calculationType)
    {
        return calculationType switch
        {
            CalculationType.BalanceFixing => DatabricksJobNames.BalanceFixing,
            CalculationType.WholesaleFixing => DatabricksJobNames.Wholesale,
            CalculationType.FirstCorrectionSettlement => DatabricksJobNames.Wholesale,
            CalculationType.SecondCorrectionSettlement => DatabricksJobNames.Wholesale,
            CalculationType.ThirdCorrectionSettlement => DatabricksJobNames.Wholesale,
            CalculationType.Aggregation => DatabricksJobNames.Wholesale,
            _ => throw new ArgumentOutOfRangeException(nameof(calculationType), calculationType, null),
        };
    }

    private async Task<Job> GetSettlementReportsJobAsync(string jobName)
    {
        var settlementJob = await _jobsApiClient.Jobs
            .ListPageable(name: jobName)
            .SingleAsync()
            .ConfigureAwait(false);

        return await _jobsApiClient.Jobs.Get(settlementJob.JobId).ConfigureAwait(false);
    }

    private RunParameters CreateParameters(SettlementReportRequestDto request)
    {
        var gridAreas = string.Join(", ", request.Filter.GridAreas.Select(c => c.Key));

        var jobParameters = new List<string>
        {
            $"--grid-areas=[{gridAreas}]",
            $"--period-start-datetime={request.Filter.PeriodStart}",
            $"--period-end-datetime={request.Filter.PeriodEnd}",
            $"--calculation-type={CalculationTypeMapper.ToDeltaTableValue(request.Filter.CalculationType)}",
        };

        return RunParameters.CreatePythonParams(jobParameters);
    }

    private static JobRunStatus ConvertJobStatus(Run jobRun)
    {
        if (jobRun.State == null)
        {
            return JobRunStatus.Queued;
        }

        if (jobRun.State.ResultState == RunResultState.SUCCESS && jobRun.IsCompleted)
        {
            return JobRunStatus.Completed;
        }

        if (jobRun.State.ResultState is RunResultState.FAILED or RunResultState.TIMEDOUT or RunResultState.CANCELED or RunResultState.UPSTREAM_FAILED or RunResultState.UPSTREAM_CANCELED)
        {
            return JobRunStatus.Failed;
        }

        return jobRun.State.LifeCycleState switch
        {
            RunLifeCycleState.RUNNING => JobRunStatus.Running,
            RunLifeCycleState.QUEUED or RunLifeCycleState.PENDING => JobRunStatus.Queued,
            RunLifeCycleState.TERMINATED => JobRunStatus.Canceled,
            RunLifeCycleState.INTERNAL_ERROR => JobRunStatus.Failed,
            _ => JobRunStatus.Queued,
        };
    }
}