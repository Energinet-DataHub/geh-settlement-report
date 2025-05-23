using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Commands;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;

public interface IRequestMeasurementsReportJobHandler
{
    /// <summary>
    /// Request a measurements report job
    /// </summary>
    /// <param name="request">An object containing the parameters of for the report request</param>
    /// <returns>A JobRunId value representing the run id of the requested measurements report.</returns>
    Task<JobRunId> HandleAsync(RequestMeasurementsReportCommand request);
}
