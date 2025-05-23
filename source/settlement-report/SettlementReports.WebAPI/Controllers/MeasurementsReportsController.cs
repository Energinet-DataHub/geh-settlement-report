using System.Net.Mime;
using Energinet.DataHub.RevisionLog.Integration.WebApi;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SettlementReports.WebAPI.Controllers;

[ApiController]
[Route("measurements-reports")]
public class MeasurementsReportsController
    : ControllerBase
{
    [HttpPost]
    [Route("RequestMeasurementsReport")]
    [Authorize]
    [EnableRevision("RequestMeasurementsReportAPI", typeof(MeasurementsReportRequestDto))]
    public ActionResult<long> RequestMeasurementsReport(
        [FromBody] MeasurementsReportRequestDto measurementsReportRequest)
    {
        return Ok(42);
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
    public ActionResult DownloadFileAsync([FromBody] ReportRequestId requestId)
    {
        using var stream = new MemoryStream();
        return new FileStreamResult(stream, MediaTypeNames.Application.Octet);
    }

    [HttpPost]
    [Route("cancel")]
    [Authorize]
    [EnableRevision("CancelSettlementReportAPI")]
    public ActionResult CancelSettlementReport([FromBody] ReportRequestId requestId)
    {
        return NoContent();
    }
}
