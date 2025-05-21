using System.Net.Mime;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.RevisionLog.Integration.WebApi;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Commands;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Security;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SettlementReports.WebAPI.Controllers.Mappers;

namespace SettlementReports.WebAPI.Controllers;

[ApiController]
[Route("measurements-reports")]
public class MeasurementsReportsController
    : ControllerBase
{
    private readonly IUserContext<FrontendUser> _userContext;
    private readonly IRequestMeasurementsReportJobHandler _requestMeasurementsReportJobHandler;

    public MeasurementsReportsController(
        IUserContext<FrontendUser> userContext,
        IRequestMeasurementsReportJobHandler requestMeasurementsReportJobHandler)
    {
        _userContext = userContext;
        _requestMeasurementsReportJobHandler = requestMeasurementsReportJobHandler;
    }

    [HttpPost]
    [Route("RequestMeasurementsReport")]
    [Authorize]
    [EnableRevision("RequestMeasurementsReportAPI", typeof(MeasurementsReportRequestDto))]
    public async Task<ActionResult<long>> RequestMeasurementsReport(
        [FromBody] MeasurementsReportRequestDto reportRequest)
    {
        var actorNumber = _userContext.CurrentUser.Actor.ActorNumber;
        var marketRole = MarketRoleMapper.MapToMarketRole(_userContext.CurrentUser.Actor.MarketRole);

        var requestCommand = new RequestMeasurementsReportCommand(
            reportRequest,
            _userContext.CurrentUser.UserId,
            _userContext.CurrentUser.Actor.ActorId,
            _userContext.CurrentUser.MultiTenancy,
            actorNumber,
            marketRole);

        var result = await _requestMeasurementsReportJobHandler.HandleAsync(requestCommand).ConfigureAwait(false);

        return Ok(result.Id);
    }

    [HttpGet]
    [Route("list")]
    [Authorize]
    [EnableRevision("ListSettlementReportsAPI", typeof(RequestedSettlementReportDto))]
    public IEnumerable<RequestedSettlementReportDto> ListSettlementReports()
    {
        return new List<RequestedSettlementReportDto>();
    }

    [HttpPost]
    [Route("download")]
    [Authorize]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [EnableRevision("DownloadSettlementReportAPI", typeof(RequestedSettlementReportDto))]
    public ActionResult DownloadFileAsync([FromBody] ReportRequestId reportId)
    {
        using var stream = new MemoryStream();
        return new FileStreamResult(stream, MediaTypeNames.Application.Octet);
    }

    [HttpPost]
    [Route("cancel")]
    [Authorize]
    [EnableRevision("CancelSettlementReportAPI")]
    public ActionResult CancelSettlementReport([FromBody] ReportRequestId reportId)
    {
        return NoContent();
    }
}
