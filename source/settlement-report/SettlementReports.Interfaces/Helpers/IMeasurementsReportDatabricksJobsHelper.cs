using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;

namespace Energinet.DataHub.Reports.Interfaces.Helpers;

public interface IMeasurementsReportDatabricksJobsHelper
{
    Task<JobRunId> RunJobAsync(
        MeasurementsReportRequestDto request,
        ReportRequestId reportRequestId,
        string actorGln);

    Task CancelAsync(long reportJobRunId);
}
