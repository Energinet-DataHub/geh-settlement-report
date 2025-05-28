namespace Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;

public sealed record GeneratedSettlementReportDto(
    ReportRequestId RequestId,
    string ReportFileName,
    IEnumerable<GeneratedSettlementReportFileDto> TemporaryFiles);
