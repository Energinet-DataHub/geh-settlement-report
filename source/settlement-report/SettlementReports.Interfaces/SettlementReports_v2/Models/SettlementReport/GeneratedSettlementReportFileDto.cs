namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

public sealed record GeneratedSettlementReportFileDto(
    ReportRequestId RequestId,
    SettlementReportPartialFileInfo FileInfo,
    string StorageFileName);
