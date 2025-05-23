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
using Energinet.DataHub.SettlementReport.Application.Handlers;
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
        ReportRequestId reportId)
    {
        var job = await GetJobAsync(DatabricksJobNames.MeasurementsReport).ConfigureAwait(false);
        var runParameters = CreateParameters(request, reportId);

        var runId = await _jobsApiClient.Jobs.RunNow(job.JobId, runParameters).ConfigureAwait(false);
        return new JobRunId(runId);
    }

    // TODO BJM: Share with SettlementReportDatabricksJobsHelper
    private async Task<Job> GetJobAsync(string jobName)
    {
        var settlementJob = await _jobsApiClient.Jobs
            .ListPageable(name: jobName)
            .SingleAsync()
            .ConfigureAwait(false);

        return await _jobsApiClient.Jobs.Get(settlementJob.JobId).ConfigureAwait(false);
    }

    private RunParameters CreateParameters(MeasurementsReportRequestDto request, ReportRequestId reportId)
    {
        var gridAreas = string.Join(", ", request.Filter.GridAreas);

        var jobParameters = new List<string>
        {
            $"--report-id={reportId.Id}",
            $"--grid-areas={gridAreas}",
            $"--period-start={request.Filter.PeriodStart.ToInstant()}",
            $"--period-end={request.Filter.PeriodEnd.ToInstant()}",
        };

        return RunParameters.CreatePythonParams(jobParameters);
    }
}
