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
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using NodaTime;

namespace Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports;

internal sealed class SettlementReportRequestTrigger
{
    private readonly IUserContext<FrontendUser> _userContext;
    private readonly ISettlementReportInitializeHandler _settlementReportInitializeHandler;
    private readonly IRevisionLogClient _revisionLogClient;

    public SettlementReportRequestTrigger(
        IUserContext<FrontendUser> userContext,
        ISettlementReportInitializeHandler settlementReportInitializeHandler,
        IRevisionLogClient revisionLogClient)
    {
        _userContext = userContext;
        _settlementReportInitializeHandler = settlementReportInitializeHandler;
        _revisionLogClient = revisionLogClient;
    }

    [Function(nameof(RequestSettlementReport))]
    public async Task<HttpResponseData> RequestSettlementReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        [FromBody] SettlementReportRequestDto settlementReportRequest,
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
                    activity: "RequestSettlementReport",
                    origin: nameof(SettlementReportRequestTrigger),
                    payload: System.Text.Json.JsonSerializer.Serialize(settlementReportRequest)))
            .ConfigureAwait(false);

        var marketRole = _userContext.CurrentUser.Actor.MarketRole switch
        {
            FrontendActorMarketRole.Other => MarketRole.Other,
            FrontendActorMarketRole.GridAccessProvider => MarketRole.GridAccessProvider,
            FrontendActorMarketRole.EnergySupplier => MarketRole.EnergySupplier,
            FrontendActorMarketRole.SystemOperator => MarketRole.SystemOperator,
            FrontendActorMarketRole.DataHubAdministrator => MarketRole.DataHubAdministrator,
            _ => throw new ArgumentOutOfRangeException(nameof(_userContext.CurrentUser.Actor.MarketRole)),
        };

        if (_userContext.CurrentUser is { MultiTenancy: true, Actor.MarketRole: FrontendActorMarketRole.DataHubAdministrator } &&
            settlementReportRequest.MarketRoleOverride.HasValue)
        {
            marketRole = settlementReportRequest.MarketRoleOverride.Value;
        }

        if (marketRole == MarketRole.EnergySupplier && string.IsNullOrWhiteSpace(settlementReportRequest.Filter.EnergySupplier))
        {
            settlementReportRequest = settlementReportRequest with
            {
                Filter = settlementReportRequest.Filter with
                {
                    EnergySupplier = _userContext.CurrentUser.Actor.ActorNumber,
                },
            };
        }

        if (!IsValid(settlementReportRequest, marketRole))
        {
            return req.CreateResponse(HttpStatusCode.Forbidden);
        }

        if (settlementReportRequest.Filter.CalculationType != CalculationType.BalanceFixing)
        {
            if (settlementReportRequest.Filter.GridAreas.Any(kv => kv.Value is null))
                return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var chargeOwnerId = marketRole is MarketRole.GridAccessProvider or MarketRole.SystemOperator
            ? _userContext.CurrentUser.Actor.ActorNumber
            : null;

        var actorInfo = new SettlementReportRequestedByActor(marketRole, chargeOwnerId);

        var instanceId = await client
            .ScheduleNewOrchestrationInstanceAsync(nameof(SettlementReportOrchestration.OrchestrateSettlementReport), new SettlementReportRequestInput(settlementReportRequest, actorInfo))
            .ConfigureAwait(false);

        var requestId = new SettlementReportRequestId(instanceId);

        await _settlementReportInitializeHandler
            .InitializeAsync(
                _userContext.CurrentUser.UserId,
                _userContext.CurrentUser.Actor.ActorId,
                _userContext.CurrentUser.MultiTenancy,
                requestId,
                settlementReportRequest)
            .ConfigureAwait(false);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response
            .WriteAsJsonAsync(new SettlementReportHttpResponse(requestId))
            .ConfigureAwait(false);

        return response;
    }

    private bool IsValid(SettlementReportRequestDto req, MarketRole marketRole)
    {
        if (_userContext.CurrentUser.MultiTenancy)
        {
            return true;
        }

        if (marketRole == MarketRole.GridAccessProvider)
        {
            if (!string.IsNullOrWhiteSpace(req.Filter.EnergySupplier))
            {
                return false;
            }

            return req.Filter.GridAreas.All(x => _userContext.CurrentUser.Actor.GridAreas.Contains(x.Key));
        }

        if (marketRole == MarketRole.EnergySupplier)
        {
            return req.Filter.EnergySupplier == _userContext.CurrentUser.Actor.ActorNumber;
        }

        if (marketRole == MarketRole.SystemOperator &&
            req.Filter.CalculationType != CalculationType.BalanceFixing &&
            req.Filter.CalculationType != CalculationType.Aggregation)
        {
            return true;
        }

        return false;
    }
}
