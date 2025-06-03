namespace Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

public sealed record GeneratedSettlementReportDto(
    ReportRequestId RequestId,
    string ReportFileName,
    IEnumerable<GeneratedSettlementReportFileDto> TemporaryFiles);
