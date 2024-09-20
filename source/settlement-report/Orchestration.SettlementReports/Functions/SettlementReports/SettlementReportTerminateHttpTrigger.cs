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
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using NodaTime;

namespace Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports;

internal sealed class SettlementReportTerminateHttpTrigger
{
    private readonly IUserContext<FrontendUser> _userContext;
    private readonly IGetSettlementReportsHandler _getSettlementReportsHandler;
    private readonly IUpdateFailedSettlementReportsHandler _updateFailedSettlementReportsHandler;
    private readonly IRevisionLogClient _revisionLogClient;

    public SettlementReportTerminateHttpTrigger(
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

    [Function(nameof(TerminateSettlementReport))]
    public async Task<HttpResponseData> TerminateSettlementReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequestData req,
        [FromBody] SettlementReportRequestId settlementReportRequestId,
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
                    activity: "TerminateSettlementReport",
                    origin: nameof(SettlementReportTerminateHttpTrigger),
                    payload: string.Empty))
            .ConfigureAwait(false);

        var settlementReport = (await _getSettlementReportsHandler.GetAsync().ConfigureAwait(false)).FirstOrDefault(x => x.RequestId == settlementReportRequestId);

        if (settlementReport is null)
            return req.CreateResponse(HttpStatusCode.NotFound);

        var instanceInfo = await client
            .GetInstanceAsync(settlementReport.RequestId.Id, getInputsAndOutputs: true)
            .ConfigureAwait(false);

        if (instanceInfo == null || instanceInfo.RuntimeStatus
                is OrchestrationRuntimeStatus.Running
                or OrchestrationRuntimeStatus.Pending
                or OrchestrationRuntimeStatus.Suspended)
        {
            await client.TerminateInstanceAsync(settlementReport.RequestId.Id).ConfigureAwait(false);
        }

        await _updateFailedSettlementReportsHandler
            .UpdateFailedReportAsync(settlementReportRequestId)
            .ConfigureAwait(false);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
