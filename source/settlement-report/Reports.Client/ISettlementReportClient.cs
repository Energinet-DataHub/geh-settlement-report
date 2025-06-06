using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

namespace Energinet.DataHub.Reports.Client;

/// <summary>
/// Interface of client for working with the settlement reports.
/// </summary>
public interface ISettlementReportClient
{
    /// <summary>
    /// Requests generation of a new settlement report.
    /// </summary>
    /// <returns>The job id.</returns>
    Task<JobRunId> RequestAsync(SettlementReportRequestDto requestDto, CancellationToken cancellationToken);

    /// <summary>
    /// Downloads the settlement report with the specified id.
    /// </summary>
    /// <returns>The stream to the report.</returns>
    Task<Stream> DownloadAsync(ReportRequestId requestId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of all settlement reports visible to the current user.
    /// </summary>
    /// <returns>A list of settlement reports.</returns>
    Task<IEnumerable<RequestedSettlementReportDto>> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Cancels the settlement report with the specified id.
    /// </summary>
    Task CancelAsync(ReportRequestId requestId, CancellationToken cancellationToken);
}
