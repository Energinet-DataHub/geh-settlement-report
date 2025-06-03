namespace Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

public sealed record GeneratedSettlementReportFileDto(
    ReportRequestId RequestId,
    SettlementReportPartialFileInfo FileInfo,
    string StorageFileName);
