using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.States;

public class SettlementReportScenarioState
{
    public SettlementReportRequestDto? SettlementReportRequestDto { get; set; }

    public JobRunId? JobRunId { get; set; }
}
