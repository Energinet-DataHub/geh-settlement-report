using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.MeasurementsReport;

namespace Energinet.DataHub.SettlementReport.Interfaces.Helpers;

public interface IMeasurementsReportDatabricksJobsHelper
{
    Task<JobRunId> RunJobAsync(
        MeasurementsReportRequestDto request,
        ReportRequestId reportId);
}
