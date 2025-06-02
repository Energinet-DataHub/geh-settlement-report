using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public interface IMeasurementsReportFileService
{
    Task<Stream> DownloadAsync(ReportRequestId requestId);
}
