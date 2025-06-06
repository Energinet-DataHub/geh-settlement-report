using Energinet.DataHub.Reports.Abstractions.Model;

namespace Energinet.DataHub.Reports.Application.SettlementReports;

public interface ISettlementReportFileRepository
{
    Task DeleteAsync(ReportRequestId reportRequestId, string fileName);
}
