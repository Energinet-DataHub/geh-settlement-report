using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

/// <summary>
/// The service for listing measurements reports.
/// </summary>
public interface IListMeasurementsReportService
{
    /// <summary>
    /// List all measurements reports for a given actor
    /// </summary>
    /// <param name="actorId">The actorId to return reports for</param>
    /// <returns>A list of measurements reports with metadata for a specific actor.</returns>
    Task<IEnumerable<RequestedMeasurementsReportDto>> GetAsync(Guid actorId);
}
