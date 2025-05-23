using System.Net.Mime;
using Energinet.DataHub.RevisionLog.Integration.WebApi;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.MeasurementsReport;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.SettlementReport;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Commands;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SettlementReports.WebAPI.Controllers;

[ApiController]
[Route("measurements-reports")]
public class MeasurementsReportsController
    : ControllerBase
{
    private readonly IRequestMeasurementsReportJobHandler _requestMeasurementsReportJobHandler;

    public MeasurementsReportsController(IRequestMeasurementsReportJobHandler requestMeasurementsReportJobHandler)
    {
        _requestMeasurementsReportJobHandler = requestMeasurementsReportJobHandler;
    }

    [HttpPost]
    [Route("request")]
    [Authorize]
    [EnableRevision("RequestMeasurementsReportAPI", typeof(MeasurementsReportRequestDto))]
    public async Task<ActionResult<long>> RequestMeasurementsReport(
        [FromBody] MeasurementsReportRequestDto reportRequest)
    {
        var requestCommand = new RequestMeasurementsReportCommand(reportRequest);

        var result = await _requestMeasurementsReportJobHandler.HandleAsync(requestCommand).ConfigureAwait(false);

        return Ok(result.Id);
    }

    [HttpGet]
    [Route("list")]
    [Authorize]
    [EnableRevision("ListMeasurementsReportsAPI", typeof(RequestedMeasurementsReportDto))]
    public IEnumerable<RequestedMeasurementsReportDto> ListMeasurementsReports()
    {
        return new List<RequestedMeasurementsReportDto>();
    }

    [HttpPost]
    [Route("download")]
    [Authorize]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [EnableRevision("DownloadMeasurementsReportAPI", typeof(RequestedMeasurementsReportDto))]
    public ActionResult DownloadFileAsync([FromBody] ReportRequestId reportId)
    {
        using var stream = new MemoryStream();
        return new FileStreamResult(stream, MediaTypeNames.Application.Octet);
    }

    [HttpPost]
    [Route("cancel")]
    [Authorize]
    [EnableRevision("CancelMeasurementsReportAPI")]
    public ActionResult CancelMeasurementsReport([FromBody] ReportRequestId reportId)
    {
        return NoContent();
    }
}
