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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Reports.Application;
using Energinet.DataHub.Reports.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Microsoft.Azure.Databricks.Client.Models;
using NodaTime.Extensions;

namespace Energinet.DataHub.Reports.Infrastructure.Helpers;

public class SettlementReportDatabricksJobsHelper : ISettlementReportDatabricksJobsHelper
{
    private readonly IJobsApiClient _jobsApiClient;

    public SettlementReportDatabricksJobsHelper(IJobsApiClient jobsApiClient)
    {
        _jobsApiClient = jobsApiClient;
    }

    public async Task<JobRunId> RunJobAsync(
        SettlementReportRequestDto request,
        MarketRole marketRole,
        ReportRequestId reportRequestId,
        string actorGln)
    {
        var job = await GetSettlementReportsJobAsync(GetJobName(request.Filter.CalculationType)).ConfigureAwait(false);
        return new JobRunId(await _jobsApiClient.Jobs.RunNow(job.JobId, CreateParameters(request, marketRole, reportRequestId, actorGln)).ConfigureAwait(false));
    }

    public async Task<JobRunWithStatusAndEndTime> GetJobRunAsync(long jobRunId)
    {
        var jobRun = await _jobsApiClient.Jobs.RunsGet(jobRunId, false).ConfigureAwait(false);
        return new JobRunWithStatusAndEndTime(ConvertJobStatus(jobRun.Item1), jobRun.Item1.EndTime);
    }

    public Task CancelAsync(long jobRunId)
    {
        return _jobsApiClient.Jobs.RunsCancel(jobRunId);
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

    private RunParameters CreateParameters(SettlementReportRequestDto request, MarketRole marketRole, ReportRequestId reportId, string actorGln)
    {
        var gridAreas = $"{{{string.Join(", ", request.Filter.GridAreas.Select(c => $"\"{c.Key}\": \"{(c.Value is null ? string.Empty : c.Value?.Id)}\""))}}}";

        var jobParameters = new List<string>
        {
            $"--report-id={reportId.Id}",
            $"--calculation-type={CalculationTypeMapper.ToDeltaTableValue(request.Filter.CalculationType)}",
            $"--period-start={request.Filter.PeriodStart.ToInstant()}",
            $"--period-end={request.Filter.PeriodEnd.ToInstant()}",
            $"--requesting-actor-market-role={MapMarketRole(marketRole)}",
            $"--requesting-actor-id={actorGln}",
            request.Filter.CalculationType == CalculationType.BalanceFixing
                ? $"--grid-area-codes=[{string.Join(",", request.Filter.GridAreas.Select(x => x.Key))}]"
                : $"--calculation-id-by-grid-area={gridAreas}",
        };

        if (request.Filter.EnergySupplier != null)
        {
            jobParameters.Add($"--energy-supplier-ids=[{request.Filter.EnergySupplier}]");
        }

        if (request.SplitReportPerGridArea)
        {
            jobParameters.Add("--split-report-by-grid-area");
        }

        if (request.PreventLargeTextFiles)
        {
            jobParameters.Add("--prevent-large-text-files");
        }

        if (request.IncludeBasisData)
        {
            jobParameters.Add("--include-basis-data");
        }

        return RunParameters.CreatePythonParams(jobParameters);
    }

    private static string MapMarketRole(MarketRole marketRole)
    {
        return marketRole switch
        {
            MarketRole.EnergySupplier => "energy_supplier",
            MarketRole.DataHubAdministrator => "datahub_administrator",
            MarketRole.GridAccessProvider => "grid_access_provider",
            MarketRole.SystemOperator => "system_operator",
            _ => throw new ArgumentOutOfRangeException(nameof(marketRole), marketRole, $"Market role \"{marketRole}\" not supported in report generation"),
        };
    }

    private static JobRunStatus ConvertJobStatus(Run jobRun)
    {
        if (jobRun.Status == null)
        {
            return JobRunStatus.Queued;
        }

        if (jobRun.Status.State is RunStatusState.TERMINATED or RunStatusState.TERMINATING && jobRun.IsCompleted && jobRun.Status.TerminationDetails.Code is RunTerminationCode.SUCCESS)
        {
            return JobRunStatus.Completed;
        }

        if (jobRun.Status.State is RunStatusState.TERMINATED or RunStatusState.TERMINATING && jobRun.Status.TerminationDetails.Code is RunTerminationCode.CANCELED or RunTerminationCode.USER_CANCELED)
        {
            return JobRunStatus.Canceled;
        }

        if (jobRun.Status.State is RunStatusState.TERMINATED or RunStatusState.TERMINATING && jobRun.Status.TerminationDetails.Code is
                RunTerminationCode.INTERNAL_ERROR
                or RunTerminationCode.DRIVER_ERROR
                or RunTerminationCode.CLOUD_FAILURE
                or RunTerminationCode.CLUSTER_ERROR
                or RunTerminationCode.RUN_EXECUTION_ERROR
                or RunTerminationCode.LIBRARY_INSTALLATION_ERROR)
        {
            return JobRunStatus.Failed;
        }

        return jobRun.Status.State switch
        {
            RunStatusState.PENDING or RunStatusState.QUEUED => JobRunStatus.Queued,
            RunStatusState.RUNNING => JobRunStatus.Running,
            _ => JobRunStatus.Queued,
        };
    }
}
