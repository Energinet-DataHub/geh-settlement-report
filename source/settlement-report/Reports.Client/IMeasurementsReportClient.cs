using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;

namespace Energinet.DataHub.Reports.Client;

/// <summary>
/// Interface of client for working with the measurements reports.
/// </summary>
public interface IMeasurementsReportClient
{
 /// <summary>
    /// Requests generation of a new measurements report.
    /// </summary>
    /// <returns>The job id.</returns>
    Task<JobRunId> RequestAsync(MeasurementsReportRequestDto requestDto, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of all measurements reports visible to the current user.
    /// </summary>
    /// <returns>A list of measurements reports.</returns>
    Task<IEnumerable<RequestedMeasurementsReportDto>> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Downloads the measurements report with the specified id.
    /// </summary>
    /// <returns>The stream to the report.</returns>
    Task<Stream> DownloadAsync(ReportRequestId requestId, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels the measurements report with the specified id.
    /// </summary>
    Task CancelAsync(ReportRequestId requestId, CancellationToken cancellationToken);
}
