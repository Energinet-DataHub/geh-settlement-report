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

using System.Net;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.RevisionLog.Integration;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Security;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Telemetry;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using NodaTime;

namespace Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports;

internal sealed class SettlementReportListHttpTrigger
{
    private readonly IUserContext<FrontendUser> _userContext;
    private readonly IGetSettlementReportsHandler _getSettlementReportsHandler;
    private readonly IUpdateFailedSettlementReportsHandler _updateFailedSettlementReportsHandler;
    private readonly IRevisionLogClient _revisionLogClient;

    public SettlementReportListHttpTrigger(
        IUserContext<FrontendUser> userContext,
        IGetSettlementReportsHandler getSettlementReportsHandler,
        IUpdateFailedSettlementReportsHandler updateFailedSettlementReportsHandler,
        IRevisionLogClient revisionLogClient)
    {
        _userContext = userContext;
        _getSettlementReportsHandler = getSettlementReportsHandler;
        _updateFailedSettlementReportsHandler = updateFailedSettlementReportsHandler;
        _revisionLogClient = revisionLogClient;
    }

    [Function(nameof(ListSettlementReports))]
    public async Task<HttpResponseData> ListSettlementReports(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        await _revisionLogClient.LogAsync(
                new RevisionLogEntry(
                    actorId: _userContext.CurrentUser.Actor.ActorId,
                    userId: _userContext.CurrentUser.UserId,
                    logId: Guid.NewGuid(),
                    systemId: SubsystemInformation.Id,
                    occurredOn: SystemClock.Instance.GetCurrentInstant(),
                    activity: "ListSettlementReports",
                    origin: nameof(SettlementReportListHttpTrigger),
                    payload: string.Empty))
            .ConfigureAwait(false);

        var allowedSettlementReports = await GetAllowedSettlementReportsAsync()
            .ConfigureAwait(false);

        var settlementReportsWithNewestStatus = await CheckStatusOfSettlementReportsAsync(
                client,
                allowedSettlementReports)
            .ConfigureAwait(false);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response
            .WriteAsJsonAsync(settlementReportsWithNewestStatus)
            .ConfigureAwait(false);

        return response;
    }

    private Task<IEnumerable<RequestedSettlementReportDto>> GetAllowedSettlementReportsAsync()
    {
        if (_userContext.CurrentUser.MultiTenancy)
            return _getSettlementReportsHandler.GetAsync();

        return _getSettlementReportsHandler.GetAsync(_userContext.CurrentUser.Actor.ActorId);
    }

    private async Task<IEnumerable<RequestedSettlementReportDto>> CheckStatusOfSettlementReportsAsync(
        DurableTaskClient durableTaskClient,
        IEnumerable<RequestedSettlementReportDto> settlementReports)
    {
        var finalSettlementReports = new List<RequestedSettlementReportDto>();

        foreach (var settlementReport in settlementReports)
        {
            var updatedReport = settlementReport;
            if (settlementReport.JobId == null)
            {
                var instanceInfo = await durableTaskClient
                    .GetInstanceAsync(settlementReport.RequestId.Id, getInputsAndOutputs: true)
                    .ConfigureAwait(false);

                if (instanceInfo == null)
                {
                    // If the orchestration instance is not found, we assume it is running on the other orchestration,
                    // either the heavy or the light one
                    continue;
                }

                if (instanceInfo.RuntimeStatus
                        is not OrchestrationRuntimeStatus.Running
                        and not OrchestrationRuntimeStatus.Pending
                        and not OrchestrationRuntimeStatus.Suspended)
                {
                    await _updateFailedSettlementReportsHandler
                        .UpdateFailedReportAsync(settlementReport.RequestId)
                        .ConfigureAwait(false);

                    updatedReport = settlementReport with { Status = SettlementReportStatus.Failed };
                }
                else
                {
                    var customStatus = instanceInfo.ReadCustomStatusAs<OrchestrateSettlementReportMetadata>();
                    if (customStatus != null)
                    {
                        updatedReport = updatedReport with { Progress = customStatus.OrchestrationProgress };
                    }
                }

                finalSettlementReports.Add(updatedReport);
            }
        }

        return finalSettlementReports;
    }
}
