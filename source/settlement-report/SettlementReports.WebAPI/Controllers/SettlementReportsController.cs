using System.Net.Mime;
using Azure;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;
using Energinet.DataHub.Reports.Application.SettlementReports.Commands;
using Energinet.DataHub.Reports.Application.SettlementReports.Handlers;
using Energinet.DataHub.Reports.Application.SettlementReports.Services;
using Energinet.DataHub.Reports.Common.Infrastructure.Security;
using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;
using Energinet.DataHub.Reports.WebAPI.Controllers.Mappers;
using Energinet.DataHub.RevisionLog.Integration.WebApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Reports.WebAPI.Controllers;

[ApiController]
[Route("settlement-reports")]
public class SettlementReportsController
    : ControllerBase
{
    private readonly IRequestSettlementReportJobHandler _requestSettlementReportJobHandler;
    private readonly IListSettlementReportJobsHandler _listSettlementReportJobsHandler;
    private readonly ISettlementReportFileService _downloadHandler;
    private readonly ICancelSettlementReportJobHandler _cancelSettlementReportJobHandler;
    private readonly IUserContext<FrontendUser> _userContext;

    public SettlementReportsController(
        IRequestSettlementReportJobHandler requestSettlementReportJobHandler,
        IUserContext<FrontendUser> userContext,
        IListSettlementReportJobsHandler listSettlementReportJobsHandler,
        ISettlementReportFileService downloadHandler,
        ICancelSettlementReportJobHandler cancelSettlementReportJobHandler)
    {
        _requestSettlementReportJobHandler = requestSettlementReportJobHandler;
        _userContext = userContext;
        _listSettlementReportJobsHandler = listSettlementReportJobsHandler;
        _downloadHandler = downloadHandler;
        _cancelSettlementReportJobHandler = cancelSettlementReportJobHandler;
    }

    [HttpPost]
    [Route("RequestSettlementReport")]
    [Authorize(Roles = "settlement-reports:manage")]
    [EnableRevision(activityName: "RequestSettlementReportAPI", entityType: typeof(SettlementReportRequestDto))]
    public async Task<ActionResult<long>> RequestSettlementReport([FromBody] SettlementReportRequestDto settlementReportRequest)
    {
        var actorNumber = _userContext.CurrentUser.Actor.ActorNumber;
        var marketRole = MarketRoleMapper.MapToMarketRole(_userContext.CurrentUser.Actor.MarketRole);

        if (_userContext.CurrentUser is { MultiTenancy: true, Actor.MarketRole: FrontendActorMarketRole.DataHubAdministrator } &&
            settlementReportRequest is { MarketRoleOverride: not null, ActorNumberOverride: not null })
        {
            marketRole = settlementReportRequest.MarketRoleOverride.Value;
            actorNumber = settlementReportRequest.ActorNumberOverride;
        }

        if (marketRole == MarketRole.EnergySupplier && string.IsNullOrWhiteSpace(settlementReportRequest.Filter.EnergySupplier))
        {
            settlementReportRequest = settlementReportRequest with
            {
                Filter = settlementReportRequest.Filter with
                {
                    EnergySupplier = actorNumber,
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

        var requestCommand = new RequestSettlementReportCommand(
            settlementReportRequest,
            _userContext.CurrentUser.UserId,
            _userContext.CurrentUser.Actor.ActorId,
            _userContext.CurrentUser.MultiTenancy,
            actorNumber,
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
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [EnableRevision(activityName: "DownloadSettlementReportAPI", entityType: typeof(RequestedSettlementReportDto))]
    public async Task<ActionResult> DownloadFileAsync([FromBody] ReportRequestId requestId)
    {
        try
        {
            var stream = await _downloadHandler
                .DownloadAsync(
                    requestId,
                    _userContext.CurrentUser.Actor.ActorId,
                    _userContext.CurrentUser.MultiTenancy)
                .ConfigureAwait(false);

            return new FileStreamResult(stream, MediaTypeNames.Application.Octet);
        }
        catch (Exception ex) when (ex is InvalidOperationException or RequestFailedException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Route("cancel")]
    [Authorize(Roles = "settlement-reports:manage")]
    [EnableRevision(activityName: "CancelSettlementReportAPI")]
    public async Task<ActionResult> CancelSettlementReport([FromBody] ReportRequestId requestId)
    {
        try
        {
            var command = new CancelSettlementReportCommand(requestId, _userContext.CurrentUser.UserId);

            await _cancelSettlementReportJobHandler.HandleAsync(command).ConfigureAwait(false);

            return NoContent();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ClientApiException)
        {
            return BadRequest();
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
