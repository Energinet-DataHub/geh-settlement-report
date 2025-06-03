namespace Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

public sealed record SettlementReportRequestDto(
    bool SplitReportPerGridArea,
    bool PreventLargeTextFiles,
    bool IncludeBasisData,
    bool IncludeMonthlyAmount,
    SettlementReportRequestFilterDto Filter,
    string? ActorNumberOverride = null,
    MarketRole? MarketRoleOverride = null);
