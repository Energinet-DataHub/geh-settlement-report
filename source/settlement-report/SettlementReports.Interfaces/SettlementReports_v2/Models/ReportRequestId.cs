namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

/// <summary>
/// The ReportRequestId is a unique identifier for both a report and the request to create the report.
/// It is used publicly in the web API.
/// </summary>
/// <param name="Id"></param>
public sealed record ReportRequestId(string Id);
