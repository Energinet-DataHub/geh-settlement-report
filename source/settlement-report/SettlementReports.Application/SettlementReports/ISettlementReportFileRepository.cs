using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Application.SettlementReports;

public interface ISettlementReportFileRepository
{
    Task DeleteAsync(ReportRequestId reportRequestId, string fileName);
}
