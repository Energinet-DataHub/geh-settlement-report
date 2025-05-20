namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

public sealed record MeasurementsReportRequestDto(
    MeasurementsReportRequestFilterDto Filter,
    string? ActorNumberOverride,
    MarketRole? MarketRoleOverride);
