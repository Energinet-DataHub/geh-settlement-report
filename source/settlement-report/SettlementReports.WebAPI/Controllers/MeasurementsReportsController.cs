using System.Net.Mime;
using Azure;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Commands;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Handlers;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Services;
using Energinet.DataHub.Reports.Common.Infrastructure.Security;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Reports.WebAPI.Controllers;

[ApiController]
[Route("measurements-reports")]
public class MeasurementsReportsController
    : ControllerBase
{
    private readonly IMeasurementsReportFileService _fileService;
    private readonly IRequestMeasurementsReportHandler _requestHandler;
    private readonly IListMeasurementsReportService _listMeasurementsReportService;
    private readonly IMeasurementsReportService _measurementsReportService;
    private readonly IUserContext<FrontendUser> _userContext;

    public MeasurementsReportsController(
        IRequestMeasurementsReportHandler requestHandler,
        IMeasurementsReportFileService fileService,
        IListMeasurementsReportService listMeasurementsReportService,
        IMeasurementsReportService measurementsReportService,
        IUserContext<FrontendUser> userContext)
    {
        _requestHandler = requestHandler;
        _fileService = fileService;
        _listMeasurementsReportService = listMeasurementsReportService;
        _measurementsReportService = measurementsReportService;
        _userContext = userContext;
    }

    [HttpPost]
    [Route("request")]
    [Authorize]
    public async Task<ActionResult<long>> RequestMeasurementsReport(
        [FromBody] MeasurementsReportRequestDto measurementsReportRequest)
    {
        var actorGln = _userContext.CurrentUser.Actor.ActorNumber;

        var requestCommand = new RequestMeasurementsReportCommand(
            measurementsReportRequest,
            _userContext.CurrentUser.UserId,
            _userContext.CurrentUser.Actor.ActorId,
            measurementsReportRequest.IsFas,
            actorGln);

        var result = await _requestHandler.HandleAsync(requestCommand).ConfigureAwait(false);

        return Ok(result.Id);
    }

    [HttpGet]
    [Route("list")]
    [Authorize]
    public async Task<IEnumerable<RequestedMeasurementsReportDto>> ListMeasurementsReports()
    {
        return await _listMeasurementsReportService.GetAsync(_userContext.CurrentUser.Actor.ActorId).ConfigureAwait(false);
    }

    [HttpPost]
    [Route("download")]
    [Authorize]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> DownloadFileAsync([FromBody] ReportRequestId reportId)
    {
        try
        {
            var stream = await _fileService.DownloadAsync(reportId).ConfigureAwait(false);
            return new FileStreamResult(stream, MediaTypeNames.Application.Octet);
        }
        catch (Exception ex) when (ex is InvalidOperationException or RequestFailedException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Route("cancel")]
    [Authorize]
    public async Task<ActionResult> CancelMeasurementsReport([FromBody] ReportRequestId reportRequestId)
    {
        try
        {
            await _measurementsReportService
                .CancelAsync(reportRequestId, _userContext.CurrentUser.UserId)
                .ConfigureAwait(false);

            return NoContent();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ClientApiException)
        {
            return BadRequest();
        }
    }
}
