using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Application.SettlementReports;

public interface ISettlementReportFileRepository
{
    Task<bool> DeleteAsync(ReportRequestId reportRequestId, string fileName);

    Task<Stream> DownloadAsync(string fileName);
}
