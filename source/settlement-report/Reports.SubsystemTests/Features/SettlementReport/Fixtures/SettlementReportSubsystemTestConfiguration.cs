using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;
using Energinet.DataHub.Reports.SubsystemTests.Fixtures;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;

public class SettlementReportSubsystemTestConfiguration : ReportsSubsystemTestConfiguration
{
    public SettlementReportSubsystemTestConfiguration()
    {
        var calculationId = Root.GetValue<string>("EXISTING_WHOLESALE_FIXING_CALCULATION_ID") ?? throw new NullReferenceException($"Missing configuration value for EXISTING_WHOLESALE_FIXING_CALCULATION_ID");

        ExistingWholesaleFixingCalculationId = new CalculationId(new Guid(calculationId));
    }

    public CalculationId ExistingWholesaleFixingCalculationId { get; }
}
