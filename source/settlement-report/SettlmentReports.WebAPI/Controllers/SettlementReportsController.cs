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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Security;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.ImbalancePrices.Hosts.WebApi.Controllers;

[ApiController]
[Route("settlement-reports")]
public class SettlementReportsController(IUserContext<FrontendUser> userContext)
    : ControllerBase
{
    [HttpPost]
    [Route("RequestSettlementReport")]
    [Authorize]
    public async Task<ActionResult> RequestSettlementReport([FromBody] SettlementReportRequestDto settlementReportRequest)
    {
        if (userContext.CurrentUser.Actor.MarketRole == FrontendActorMarketRole.EnergySupplier && string.IsNullOrWhiteSpace(settlementReportRequest.Filter.EnergySupplier))
        {
            settlementReportRequest = settlementReportRequest with
            {
                Filter = settlementReportRequest.Filter with
                {
                    EnergySupplier = userContext.CurrentUser.Actor.ActorNumber,
                },
            };
        }

        if (!IsValid(settlementReportRequest))
        {
            return Forbid();
        }

        if (settlementReportRequest.Filter.CalculationType != CalculationType.BalanceFixing)
        {
            if (settlementReportRequest.Filter.GridAreas.Any(kv => kv.Value is null))
              return BadRequest();
        }

        var marketRole = userContext.CurrentUser.Actor.MarketRole switch
        {
            FrontendActorMarketRole.Other => MarketRole.Other,
            FrontendActorMarketRole.GridAccessProvider => MarketRole.GridAccessProvider,
            FrontendActorMarketRole.EnergySupplier => MarketRole.EnergySupplier,
            FrontendActorMarketRole.SystemOperator => MarketRole.SystemOperator,
            FrontendActorMarketRole.DataHubAdministrator => MarketRole.DataHubAdministrator,
            _ => throw new ArgumentOutOfRangeException(nameof(userContext.CurrentUser.Actor.MarketRole)),
        };

        var chargeOwnerId = marketRole is MarketRole.GridAccessProvider or MarketRole.SystemOperator
            ? userContext.CurrentUser.Actor.ActorNumber
            : null;

        var result = await Task.FromResult(1).ConfigureAwait(false);

        return Ok(result);
    }

    private bool IsValid(SettlementReportRequestDto req)
    {
        if (userContext.CurrentUser.MultiTenancy)
        {
            return true;
        }

        var marketRole = userContext.CurrentUser.Actor.MarketRole;

        if (marketRole == FrontendActorMarketRole.GridAccessProvider)
        {
            if (!string.IsNullOrWhiteSpace(req.Filter.EnergySupplier))
            {
                return false;
            }

            return req.Filter.GridAreas.All(x => userContext.CurrentUser.Actor.GridAreas.Contains(x.Key));
        }

        if (marketRole == FrontendActorMarketRole.EnergySupplier)
        {
            return req.Filter.EnergySupplier == userContext.CurrentUser.Actor.ActorNumber;
        }

        if (marketRole == FrontendActorMarketRole.SystemOperator &&
            req.Filter.CalculationType != CalculationType.BalanceFixing &&
            req.Filter.CalculationType != CalculationType.Aggregation)
        {
            return true;
        }

        return false;
    }
}
