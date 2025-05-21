namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.MeasurementsReport;

public sealed record MeasurementsReportRequestDto(
    MeasurementsReportRequestFilterDto Filter,
    string? ActorNumberOverride,
    MarketRole? MarketRoleOverride);
