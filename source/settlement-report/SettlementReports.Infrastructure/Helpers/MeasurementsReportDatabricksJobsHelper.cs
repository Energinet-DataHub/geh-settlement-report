using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Reports.Application;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Microsoft.Azure.Databricks.Client.Models;
using NodaTime.Extensions;

namespace Energinet.DataHub.Reports.Infrastructure.Helpers;

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
        string actorGln)
    {
        var job = await GetJobAsync(DatabricksJobNames.MeasurementsReport).ConfigureAwait(false);
        var runParameters = CreateParameters(request, reportRequestId, actorGln);

        var runId = await _jobsApiClient.Jobs.RunNow(job.JobId, runParameters).ConfigureAwait(false);
        return new JobRunId(runId);
    }

    public Task CancelAsync(long jobRunId)
    {
        return _jobsApiClient.Jobs.RunsCancel(jobRunId);
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
        string actorGln)
    {
        var gridAreas = string.Join(", ", request.Filter.GridAreaCodes);

        var jobParameters = new List<string>
        {
            $"--report-id={reportRequestId.Id}",
            $"--grid-area-codes=[{gridAreas}]",
            $"--period-start={request.Filter.PeriodStart.ToInstant()}",
            $"--period-end={request.Filter.PeriodEnd.ToInstant()}",
            $"--requesting-actor-id={actorGln}",
            $"--energy-supplier-ids=[{actorGln}]",
        };

        return RunParameters.CreatePythonParams(jobParameters);
    }
}
