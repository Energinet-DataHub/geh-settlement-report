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

using Azure;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Exceptions;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports.Activities;
using Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using RetryContext = Microsoft.DurableTask.RetryContext;

namespace Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports;

internal sealed class SettlementReportOrchestration
{
    private const int MaxQueuedItems = 5;

    [Function(nameof(OrchestrateSettlementReport))]
    public async Task<string> OrchestrateSettlementReport(
         [OrchestrationTrigger] TaskOrchestrationContext context,
         FunctionContext executionContext)
    {
        var settlementReportRequest = context.GetInput<SettlementReportRequestInput>();
        if (settlementReportRequest == null)
        {
            return "Error: No input specified.";
        }

        var requestId = new ReportRequestId(context.InstanceId);
        var scatterInput = new ScatterSettlementReportFilesInput(requestId, settlementReportRequest.Request, settlementReportRequest.ActorInfo);

        var dataSourceExceptionHandler = TaskOptions.FromRetryHandler(retryContext => HandleDataSourceExceptions(
                retryContext,
                executionContext.GetLogger<SettlementReportOrchestration>()));

        context.SetCustomStatus(new OrchestrateSettlementReportMetadata { OrchestrationProgress = 1 });

        var scatterResults = await context
            .CallActivityAsync<IEnumerable<SettlementReportFileRequestDto>>(
                nameof(ScatterSettlementReportFilesActivity),
                scatterInput,
                dataSourceExceptionHandler);

        context.SetCustomStatus(new OrchestrateSettlementReportMetadata { OrchestrationProgress = 10 });

        var generatedFiles = new List<GeneratedSettlementReportFileDto>();
        var orderedResults = scatterResults
            .OrderBy(x => x.PartialFileInfo.FileOffset)
            .ThenBy(x => x.PartialFileInfo.ChunkOffset)
            .ToList();

        var fileRequestTasks = orderedResults.Select(fileRequest => context
            .CallActivityAsync<GeneratedSettlementReportFileDto>(
                nameof(GenerateSettlementReportFileActivity),
                new GenerateSettlementReportFileInput(fileRequest, settlementReportRequest.ActorInfo),
                dataSourceExceptionHandler)).ToList();

        while (fileRequestTasks.Count != 0)
        {
            var doneTask = await Task.WhenAny(fileRequestTasks);
            generatedFiles.Add(await doneTask);
            fileRequestTasks.Remove(doneTask);
            context.SetCustomStatus(new OrchestrateSettlementReportMetadata
            {
                OrchestrationProgress = (80.0 * generatedFiles.Count / orderedResults.Count) + 10,
            });
        }

        var generatedSettlementReport = await context.CallActivityAsync<GeneratedSettlementReportDto>(
            nameof(GatherSettlementReportFilesActivity),
            new GatherSettlementReportFilesInput(requestId, generatedFiles));

        context.SetCustomStatus(new OrchestrateSettlementReportMetadata { OrchestrationProgress = 95 });

        await context.CallActivityAsync(
            nameof(FinalizeSettlementReportActivity),
            generatedSettlementReport);

        context.SetCustomStatus(new OrchestrateSettlementReportMetadata { OrchestrationProgress = 100 });

        return "Success";
    }

    private static bool HandleDataSourceExceptions(RetryContext retryContext, ILogger<SettlementReportOrchestration> logger)
    {
        // When running ScatterSettlementReportFilesActivity or GenerateSettlementReportFile, the call to the data source may time out for several reasons:
        // 1) The server is stopped, but requesting the data has triggered a startup. It should come online within 3 retries.
        // 2) The query for getting the data timed out. It is not known if query will succeed, but we are trying up to 6 times.
        if (retryContext.LastFailure.ErrorType == typeof(DatabricksException).FullName)
        {
            logger.LogError("Databricks data source failed. Inner exception message: {innerException}.", retryContext.LastFailure.InnerFailure?.ToString());
            return retryContext.LastAttemptNumber <= 6;
        }

        // In case the error is not related to the data source, we are retrying twice to take care of transient failures:
        // 1) From SQL.
        // 2) From BlobStorage.
        if (retryContext.LastFailure.ErrorType.Contains("SqlException") ||
            retryContext.LastFailure.ErrorType == typeof(RequestFailedException).FullName ||
            retryContext.LastFailure.ErrorType == typeof(OperationCanceledException).FullName)
        {
            return retryContext.LastAttemptNumber <= 2;
        }

        return false;
    }
}
