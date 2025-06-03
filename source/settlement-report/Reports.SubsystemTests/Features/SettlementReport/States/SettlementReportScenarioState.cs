using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.States;

public class SettlementReportScenarioState
{
    public SettlementReportRequestDto? SettlementReportRequestDto { get; set; }

    public JobRunId? JobRunId { get; set; }
}
