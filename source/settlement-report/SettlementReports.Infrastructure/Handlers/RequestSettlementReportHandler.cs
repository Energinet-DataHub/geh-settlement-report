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
using Energinet.DataHub.SettlementReport.Infrastructure.Commands;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Handlers;

public sealed class RequestSettlementReportHandler : IRequestSettlemenReportJobHandler
{
    private readonly IJobsApiClient _jobsApiClient;

    public RequestSettlementReportHandler(IJobsApiClient jobsApiClient)
    {
        _jobsApiClient = jobsApiClient;
    }

    public async Task<long> HandleAsync(RequestSettlementReportJobCommand command)
    {
        var job = await GetSettlementReportsJobAsync(command.Request.Filter.CalculationType == CalculationType.BalanceFixing
            ? DatabricksJobNames.BalanceFixing
            : DatabricksJobNames.Wholesale)
            .ConfigureAwait(false);

        var parameters = CreateParameters(command.Request);
        return await _jobsApiClient.Jobs.RunNow(job.JobId, parameters).ConfigureAwait(false);
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

    private async Task<Job> GetSettlementReportsJobAsync(string jobName)
    {
        var calculatorJob = await _jobsApiClient.Jobs
            .ListPageable(name: jobName)
            .SingleAsync()
            .ConfigureAwait(false);

        return await _jobsApiClient.Jobs.Get(calculatorJob.JobId).ConfigureAwait(false);
    }
}
