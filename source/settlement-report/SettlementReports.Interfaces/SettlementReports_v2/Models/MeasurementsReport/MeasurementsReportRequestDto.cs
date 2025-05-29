namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

public sealed record MeasurementsReportRequestDto(
    MeasurementsReportRequestFilterDto Filter,
    string? ActorNumberOverride = null,
    MarketRole? MarketRoleOverride = null);
