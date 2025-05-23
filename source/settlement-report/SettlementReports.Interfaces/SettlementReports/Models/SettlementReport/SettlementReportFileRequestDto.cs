namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.SettlementReport;

public sealed record SettlementReportFileRequestDto(
    ReportRequestId RequestId,
    SettlementReportFileContent FileContent,
    SettlementReportPartialFileInfo PartialFileInfo,
    SettlementReportRequestFilterDto RequestFilter,
    long MaximumCalculationVersion);
