namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.SettlementReport;

public sealed record GeneratedSettlementReportDto(
    ReportRequestId RequestId,
    string ReportFileName,
    IEnumerable<GeneratedSettlementReportFileDto> TemporaryFiles);
