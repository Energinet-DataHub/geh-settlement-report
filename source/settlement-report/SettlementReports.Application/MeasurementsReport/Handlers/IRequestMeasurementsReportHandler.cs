using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Commands;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Handlers;

public interface IRequestMeasurementsReportHandler
{
    /// <summary>
    /// Request a measurements report job
    /// </summary>
    /// <param name="request">An object containing the parameters of for the report request</param>
    /// <returns>A JobRunId value representing the run id of the requested measurements report.</returns>
    Task<JobRunId> HandleAsync(RequestMeasurementsReportCommand request);
}
