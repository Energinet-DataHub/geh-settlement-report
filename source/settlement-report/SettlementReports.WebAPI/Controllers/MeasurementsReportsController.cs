using Energinet.DataHub.RevisionLog.Integration.WebApi;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
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
    [EnableRevision(activityName: "RequestMeasurementsReportAPI", entityType: typeof(MeasurementsReportRequestDto))]
    public ActionResult<long> RequestMeasurementsReport([FromBody] MeasurementsReportRequestDto measurementsReportRequest)
    {
        return Ok(42);
    }
}
