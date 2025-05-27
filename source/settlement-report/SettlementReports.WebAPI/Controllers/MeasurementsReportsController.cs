using System.Net.Mime;
using Azure;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Commands;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;
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

    public MeasurementsReportsController(IRequestMeasurementsReportHandler requestHandler, IMeasurementsReportFileService fileService)
    {
        _requestHandler = requestHandler;
        _fileService = fileService;
    }

    [HttpPost]
    [Route("request")]
    [Authorize]
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
    public IEnumerable<RequestedMeasurementsReportDto> ListMeasurementsReports()
    {
        return new List<RequestedMeasurementsReportDto>();
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
    public ActionResult CancelMeasurementsReport([FromBody] ReportRequestId reportId)
    {
        return NoContent();
    }
}
