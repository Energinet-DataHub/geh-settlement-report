using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.Reports.Interfaces.Helpers;

public interface IMeasurementsReportDatabricksJobsHelper
{
    Task<JobRunId> RunJobAsync(
        MeasurementsReportRequestDto request,
        ReportRequestId reportRequestId,
        string actorGln);
}
