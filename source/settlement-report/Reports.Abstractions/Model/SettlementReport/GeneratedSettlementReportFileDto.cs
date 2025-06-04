using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

namespace Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

public sealed record GeneratedSettlementReportFileDto(
    ReportRequestId RequestId,
    SettlementReportPartialFileInfo FileInfo,
    string StorageFileName);
