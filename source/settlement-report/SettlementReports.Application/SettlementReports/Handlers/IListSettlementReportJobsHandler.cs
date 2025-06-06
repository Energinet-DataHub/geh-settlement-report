using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Handlers;

/// <summary>
/// The handler for listing settlement report jobs.
/// </summary>
public interface IListSettlementReportJobsHandler
{
    /// <summary>
    /// List all settlement report jobs
    /// </summary>
    /// <returns>A list of settlement reports with metadata.</returns>
    Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync();

    /// <summary>
    /// List all settlement report jobs for a given actor
    /// </summary>
    /// <param name="actorId">The actorId to return reports for</param>
    /// <returns>A list of settlement reports with metadata for a specific actor.</returns>
    Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync(Guid actorId);
}
