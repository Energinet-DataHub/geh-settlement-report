using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;

public interface IMeasurementsReportFileService
{
    Task<Stream> DownloadAsync(ReportRequestId requestId);
}
