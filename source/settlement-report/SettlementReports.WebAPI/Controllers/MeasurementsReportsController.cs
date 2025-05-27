using System.Net.Mime;
using Azure;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Commands;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Security;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SettlementReports.WebAPI.Controllers;

[ApiController]
[Route("measurements-reports")]
public class MeasurementsReportsController
    : ControllerBase
{
    private readonly IMeasurementsReportFileService _fileService;
    private readonly IRequestMeasurementsReportHandler _requestHandler;
    private readonly IListMeasurementsReportJobsHandler _listMeasurementsReportJobsHandler;
    private readonly IUserContext<FrontendUser> _userContext;

    public MeasurementsReportsController(IRequestMeasurementsReportHandler requestHandler, IMeasurementsReportFileService fileService, IListMeasurementsReportJobsHandler listMeasurementsReportJobsHandler, IUserContext<FrontendUser> userContext)
    {
        _requestHandler = requestHandler;
        _fileService = fileService;
        _listMeasurementsReportJobsHandler = listMeasurementsReportJobsHandler;
        _userContext = userContext;
    }

    [HttpPost]
    [Route("request")]
    [Authorize(Roles = "measurements-reports:manage")]
    public async Task<ActionResult<long>> RequestMeasurementsReport(
        [FromBody] MeasurementsReportRequestDto reportRequest)
    {
        var requestCommand = new RequestMeasurementsReportCommand(reportRequest);

        var result = await _requestHandler.HandleAsync(requestCommand).ConfigureAwait(false);

        return Ok(result.Id);
    }

    [HttpGet]
    [Route("list")]
    [Authorize]
    public async Task<IEnumerable<RequestedMeasurementsReportDto>> ListMeasurementsReports()
    {
        if (_userContext.CurrentUser.MultiTenancy)
            return await _listMeasurementsReportJobsHandler.HandleAsync().ConfigureAwait(false);

        return await _listMeasurementsReportJobsHandler.HandleAsync(_userContext.CurrentUser.Actor.ActorId).ConfigureAwait(false);
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
    [Authorize(Roles = "measurements-reports:manage")]
    public ActionResult CancelMeasurementsReport([FromBody] ReportRequestId reportId)
    {
        return NoContent();
    }
}
