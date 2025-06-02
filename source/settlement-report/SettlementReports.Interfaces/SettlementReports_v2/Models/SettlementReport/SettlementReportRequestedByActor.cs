namespace Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;

public sealed record SettlementReportRequestedByActor(MarketRole MarketRole, string? ChargeOwnerId);
