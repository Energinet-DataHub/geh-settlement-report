using Energinet.DataHub.SettlementReport.Application.Commands;
using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Commands;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;

public interface IRequestMeasurementsReportJobHandler
{
    /// <summary>
    /// Request a settlement report job
    /// </summary>
    /// <param name="request">An object containing the parameters of for the report request</param>
    /// <returns>A JobRunId value representing the run id of the requested settlement report.</returns>
    Task<JobRunId> HandleAsync(RequestMeasurementsReportCommand request);
}
