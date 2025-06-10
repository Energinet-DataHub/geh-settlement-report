using System.Net.Mime;
using Azure;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Commands;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Handlers;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Services;
using Energinet.DataHub.Reports.Common.Infrastructure.Security;
using Energinet.DataHub.Reports.WebAPI.Controllers.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Reports.WebAPI.Controllers;

[ApiController]
[Route("measurements-reports")]
public class MeasurementsReportsController
    : ControllerBase
{
    private readonly IMeasurementsReportFileService _fileService;
    private readonly IRequestMeasurementsReportHandler _requestHandler;
    private readonly IListMeasurementsReportService _listMeasurementsReportService;
    private readonly IUserContext<FrontendUser> _userContext;

    public MeasurementsReportsController(
        IRequestMeasurementsReportHandler requestHandler,
        IMeasurementsReportFileService fileService,
        IListMeasurementsReportService listMeasurementsReportService,
        IUserContext<FrontendUser> userContext)
    {
        _requestHandler = requestHandler;
        _fileService = fileService;
        _listMeasurementsReportService = listMeasurementsReportService;
        _userContext = userContext;
    }

    [HttpPost]
    [Route("request")]
    // Authorization roles correspond to the permissions (of a user role) as defined in the frontend.
    [Authorize(Roles = "measurements-reports:manage")]
    public async Task<ActionResult<long>> RequestMeasurementsReport(
        [FromBody] MeasurementsReportRequestDto measurementsReportRequest)
    {
        if (!UserHasValidMarketRole())
            return Forbid();

        if (!IsValidRequest(measurementsReportRequest))
            return BadRequest();

        var actorGln = _userContext.CurrentUser.Actor.ActorNumber;
        var marketRole = MarketRoleMapper.MapToMarketRole(_userContext.CurrentUser.Actor.MarketRole);

        if (marketRole == MarketRole.EnergySupplier && string.IsNullOrWhiteSpace(measurementsReportRequest.Filter.EnergySupplier))
        {
            measurementsReportRequest = measurementsReportRequest with
            {
                Filter = measurementsReportRequest.Filter with
                {
                    EnergySupplier = actorGln,
                },
            };
        }

        var requestCommand = new RequestMeasurementsReportCommand(
            measurementsReportRequest,
            _userContext.CurrentUser.UserId,
            _userContext.CurrentUser.Actor.ActorId,
            _userContext.CurrentUser.Actor.ActorNumber);

        var result = await _requestHandler.HandleAsync(requestCommand).ConfigureAwait(false);

        return Ok(result.Id);
    }

    [HttpGet]
    [Route("list")]
    // Authorization roles correspond to the permissions (of a user role) as defined in the frontend.
    [Authorize(Roles = "measurements-reports:manage")]
    public async Task<ActionResult<IEnumerable<RequestedMeasurementsReportDto>>> ListMeasurementsReports()
    {
        if (!UserHasValidMarketRole())
            return Forbid();

        var reports = await _listMeasurementsReportService.GetAsync(_userContext.CurrentUser.Actor.ActorId).ConfigureAwait(false);
        return Ok(reports);
    }

    [HttpPost]
    [Route("download")]
    // Authorization roles correspond to the permissions (of a user role) as defined in the frontend.
    [Authorize(Roles = "measurements-reports:manage")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> DownloadFileAsync([FromBody] ReportRequestId reportId)
    {
        if (!UserHasValidMarketRole())
            return Forbid();

        try
        {
            var stream = await _fileService.DownloadAsync(reportId, _userContext.CurrentUser.Actor.ActorId).ConfigureAwait(false);
            return new FileStreamResult(stream, MediaTypeNames.Application.Octet);
        }
        catch (Exception ex) when (ex is InvalidOperationException or RequestFailedException)
        {
            return NotFound();
        }
    }

    private bool UserHasValidMarketRole()
    {
        var marketRole = MarketRoleMapper.MapToMarketRole(_userContext.CurrentUser.Actor.MarketRole);

        // These are the supported market roles for measurements reports
        var supportedMarketRoles = new[] { MarketRole.GridAccessProvider, MarketRole.EnergySupplier };

        return supportedMarketRoles.Contains(marketRole);
    }

    private bool IsValidRequest(MeasurementsReportRequestDto req)
    {
        // Validate that the users actor has access to the grid areas specified in the request
        if (!req.Filter.GridAreaCodes.All(c => _userContext.CurrentUser.Actor.GridAreas.Contains(c)))
            return false;

        // Reject empty or "negative" periods
        if (req.Filter.PeriodEnd <= req.Filter.PeriodStart)
            return false;

        // Period must not exceed one month
        if (req.Filter.PeriodEnd > req.Filter.PeriodStart.AddMonths(1))
            return false;

        return true;
    }
}
