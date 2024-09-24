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

using System.Net.Mime;
using Azure;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.RevisionLog.Integration.WebApi;
using Energinet.DataHub.SettlementReport.Application.Commands;
using Energinet.DataHub.SettlementReport.Application.Handlers;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Security;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SettlementReports.WebAPI.Controllers;

[ApiController]
[Route("settlement-reports")]
public class SettlementReportsController
    : ControllerBase
{
    private readonly IRequestSettlementReportJobHandler _requestSettlementReportJobHandler;
    private readonly IListSettlementReportJobsHandler _listSettlementReportJobsHandler;
    private readonly ISettlementReportJobsDownloadHandler _downloadHandler;
    private readonly IUserContext<FrontendUser> _userContext;

    public SettlementReportsController(
        IRequestSettlementReportJobHandler requestSettlementReportJobHandler,
        IUserContext<FrontendUser> userContext,
        IListSettlementReportJobsHandler listSettlementReportJobsHandler,
        ISettlementReportJobsDownloadHandler downloadHandler)
    {
        _requestSettlementReportJobHandler = requestSettlementReportJobHandler;
        _userContext = userContext;
        _listSettlementReportJobsHandler = listSettlementReportJobsHandler;
        _downloadHandler = downloadHandler;
    }

    [HttpPost]
    [Route("RequestSettlementReport")]
    [Authorize(Roles = "settlement-reports:manage")]
    [EnableRevision(activityName: "RequestSettlementReportAPI", entityType: typeof(SettlementReportRequestDto))]
    public async Task<ActionResult<long>> RequestSettlementReport([FromBody] SettlementReportRequestDto settlementReportRequest)
    {
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
            return Forbid();
        }

        if (settlementReportRequest.Filter.CalculationType != CalculationType.BalanceFixing)
        {
            if (settlementReportRequest.Filter.GridAreas.Any(kv => kv.Value is null))
                return BadRequest();
        }

        var chargeOwnerId = marketRole is MarketRole.GridAccessProvider or MarketRole.SystemOperator
            ? _userContext.CurrentUser.Actor.ActorNumber
            : null;

        var requestCommand = new RequestSettlementReportCommand(
            settlementReportRequest,
            _userContext.CurrentUser.UserId,
            _userContext.CurrentUser.Actor.ActorId,
            _userContext.CurrentUser.MultiTenancy,
            chargeOwnerId,
            marketRole);

        var result = await _requestSettlementReportJobHandler.HandleAsync(requestCommand).ConfigureAwait(false);

        return Ok(result.Id);
    }

    [HttpGet]
    [Route("list")]
    [Authorize]
    [EnableRevision(activityName: "ListSettlementReportsAPI", entityType: typeof(RequestedSettlementReportDto))]
    public async Task<IEnumerable<RequestedSettlementReportDto>> ListSettlementReports()
    {
        if (_userContext.CurrentUser.MultiTenancy)
            return await _listSettlementReportJobsHandler.HandleAsync().ConfigureAwait(false);

        return await _listSettlementReportJobsHandler.HandleAsync(_userContext.CurrentUser.Actor.ActorId).ConfigureAwait(false);
    }

    [HttpPost]
    [Route("download")]
    [Authorize]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [EnableRevision(activityName: "DownloadSettlementReportAPI", entityType: typeof(RequestedSettlementReportDto))]
    public async Task<ActionResult> DownloadFileAsync([FromBody]SettlementReportRequestId requestId)
    {
        try
        {
            var stream = new MemoryStream();
            await _downloadHandler
                .DownloadReportAsync(
                    requestId,
                    () => stream,
                    _userContext.CurrentUser.Actor.ActorId,
                    _userContext.CurrentUser.MultiTenancy)
                .ConfigureAwait(false);

            return File(stream.GetBuffer(), MediaTypeNames.Application.Octet);
        }
        catch (Exception ex) when (ex is InvalidOperationException or RequestFailedException)
        {
            return NotFound();
        }
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
