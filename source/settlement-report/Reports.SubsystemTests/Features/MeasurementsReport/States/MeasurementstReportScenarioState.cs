using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.States;

public class MeasurementsReportScenarioState
{
    public MeasurementsReportRequestDto? MeasurementsReportRequestDto { get; set; }

    public JobRunId? JobRunId { get; set; }
}
