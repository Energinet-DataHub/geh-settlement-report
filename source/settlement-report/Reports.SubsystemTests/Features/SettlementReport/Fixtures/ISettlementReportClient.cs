using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;

// TODO JMG: Make this a real client including nuget, so it can be shared with BFF?

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
    /// Requests generation of a new settlement report.
    /// </summary>
    /// <returns>The job id.</returns>
    Task<JobRunId> RequestAsync(MeasurementsReportRequestDto requestDto, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of all measurements reports visible to the current user.
    /// </summary>
    /// <returns>A list of measurements reports.</returns>
    Task<IEnumerable<RequestedMeasurementsReportDto>> GetMeasurementsReportAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of all settlement reports visible to the current user.
    /// </summary>
    /// <returns>A list of settlement reports.</returns>
    Task<IEnumerable<RequestedSettlementReportDto>> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Downloads the settlement report with the specified id.
    /// </summary>
    /// <returns>The stream to the report.</returns>
    Task<Stream> DownloadAsync(ReportRequestId requestId, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels the settlement report with the specified id.
    /// </summary>
    Task CancelAsync(ReportRequestId requestId, CancellationToken cancellationToken);
}
