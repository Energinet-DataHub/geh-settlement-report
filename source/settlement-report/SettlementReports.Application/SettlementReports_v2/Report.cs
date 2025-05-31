namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public abstract class Report
{
    /// <summary>
    /// Internal (database) ID of the report.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The public ID of the report.
    /// </summary>
    public string RequestId { get; init; } = null!;
}
