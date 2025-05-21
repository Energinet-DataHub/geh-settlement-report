namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

public sealed record GeneratedSettlementReportDto(
    ReportRequestId RequestId,
    string ReportFileName,
    IEnumerable<GeneratedSettlementReportFileDto> TemporaryFiles);
