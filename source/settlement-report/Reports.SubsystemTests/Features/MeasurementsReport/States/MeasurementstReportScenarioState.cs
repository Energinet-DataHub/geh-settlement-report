using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.MeasurementsReport;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.States;

public class MeasurementsReportScenarioState
{
    public MeasurementsReportRequestDto? MeasurementsReportRequestDto { get; set; }

    public JobRunId? JobRunId { get; set; }
}
