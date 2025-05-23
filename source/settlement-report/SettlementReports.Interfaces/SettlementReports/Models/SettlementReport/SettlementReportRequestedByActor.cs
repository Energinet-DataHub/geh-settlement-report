namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.SettlementReport;

public sealed record SettlementReportRequestedByActor(MarketRole MarketRole, string? ChargeOwnerId);
