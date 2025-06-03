using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.MeasurementsReport;

namespace Energinet.DataHub.Reports.Interfaces.Helpers;

public interface IMeasurementsReportDatabricksJobsHelper
{
    Task<JobRunId> RunJobAsync(
        MeasurementsReportRequestDto request,
        ReportRequestId reportRequestId,
        string actorGln);

    Task CancelAsync(long reportJobRunId);
}
