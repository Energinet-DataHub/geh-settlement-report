namespace Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;

public sealed record GeneratedSettlementReportFileDto(
    ReportRequestId RequestId,
    SettlementReportPartialFileInfo FileInfo,
    string StorageFileName);
