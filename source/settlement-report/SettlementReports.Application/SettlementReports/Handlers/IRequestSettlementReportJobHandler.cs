using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Application.SettlementReports.Commands;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Handlers;

public interface IRequestSettlementReportJobHandler
{
    /// <summary>
    /// Request a settlement report job
    /// </summary>
    /// <param name="request">An object containing the parameters of for the report request</param>
    /// <returns>A JobRunId value representing the run id of the requested settlement report.</returns>
    Task<JobRunId> HandleAsync(RequestSettlementReportCommand request);
}
