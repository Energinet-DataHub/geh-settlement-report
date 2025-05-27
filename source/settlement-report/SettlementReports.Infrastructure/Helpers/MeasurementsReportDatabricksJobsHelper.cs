using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.SettlementReport.Application;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Microsoft.Azure.Databricks.Client.Models;
using NodaTime.Extensions;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Helpers;

public class MeasurementsReportDatabricksJobsHelper : IMeasurementsReportDatabricksJobsHelper
{
    private readonly IJobsApiClient _jobsApiClient;

    public MeasurementsReportDatabricksJobsHelper(IJobsApiClient jobsApiClient)
    {
        _jobsApiClient = jobsApiClient;
    }

    public async Task<JobRunId> RunJobAsync(
        MeasurementsReportRequestDto request,
        ReportRequestId reportRequestId,
        MarketRole marketRole,
        string actorGln)
    {
        var job = await GetJobAsync(DatabricksJobNames.MeasurementsReport).ConfigureAwait(false);
        var runParameters = CreateParameters(request, reportRequestId, marketRole, actorGln);

        var runId = await _jobsApiClient.Jobs.RunNow(job.JobId, runParameters).ConfigureAwait(false);
        return new JobRunId(runId);
    }

    private async Task<Job> GetJobAsync(string jobName)
    {
        var settlementJob = await _jobsApiClient.Jobs
            .ListPageable(name: jobName)
            .SingleAsync()
            .ConfigureAwait(false);

        return await _jobsApiClient.Jobs.Get(settlementJob.JobId).ConfigureAwait(false);
    }

    private RunParameters CreateParameters(
        MeasurementsReportRequestDto request,
        ReportRequestId reportRequestId,
        MarketRole marketRole,
        string actorGln)
    {
        var gridAreas = string.Join(", ", request.Filter.GridAreas);

        var jobParameters = new List<string>
        {
            $"--report-id={reportRequestId.Id}",
            $"--grid-areas={gridAreas}",
            $"--period-start={request.Filter.PeriodStart.ToInstant()}",
            $"--period-end={request.Filter.PeriodEnd.ToInstant()}",
            $"--requesting-actor-market-role={MapMarketRole(marketRole)}",
            $"--requesting-actor-id={actorGln}",
        };

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
            _ => throw new ArgumentOutOfRangeException(
                nameof(marketRole),
                marketRole,
                $"Market role \"{marketRole}\" not supported in report generation"),
        };
    }
}
