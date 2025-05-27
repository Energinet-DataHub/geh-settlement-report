using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;

/// <summary>
/// The handler for listing settlement report jobs.
/// </summary>
public interface IListMeasurementsReportJobsHandler
{
    /// <summary>
    /// List all settlement report jobs
    /// </summary>
    /// <returns>A list of settlement reports with metadata.</returns>
    Task<IEnumerable<RequestedMeasurementsReportDto>> HandleAsync();

    /// <summary>
    /// List all settlement report jobs for a given actor
    /// </summary>
    /// <param name="actorId">The actorId to return reports for</param>
    /// <returns>A list of settlement reports with metadata for a specific actor.</returns>
    Task<IEnumerable<RequestedMeasurementsReportDto>> HandleAsync(Guid actorId);
}
