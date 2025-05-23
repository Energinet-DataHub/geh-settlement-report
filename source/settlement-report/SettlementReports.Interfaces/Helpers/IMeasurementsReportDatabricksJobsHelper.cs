using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.SettlementReport.Interfaces.Helpers;

public interface IMeasurementsReportDatabricksJobsHelper
{
    Task<JobRunId> RunJobAsync(
        MeasurementsReportRequestDto request,
        ReportRequestId reportId);
}
