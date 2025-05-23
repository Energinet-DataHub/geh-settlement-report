namespace Energinet.DataHub.SettlementReport.Application;

/// <summary>
/// These names are defined in infrastructure.
/// </summary>
public static class DatabricksJobNames
{
    public const string BalanceFixing = "SettlementReportBalanceFixing";
    public const string Wholesale = "SettlementReportWholesaleCalculations";
    public const string MeasurementsReport = "MeasurementsReport";
}
