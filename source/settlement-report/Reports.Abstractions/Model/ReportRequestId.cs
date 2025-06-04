namespace Energinet.DataHub.Reports.Abstractions.Model;

/// <summary>
/// The ReportRequestId is a unique identifier for both a report and the request to create the report.
/// It is used publicly in the web API.
/// </summary>
/// <param name="Id">id</param>
public sealed record ReportRequestId(string Id);
