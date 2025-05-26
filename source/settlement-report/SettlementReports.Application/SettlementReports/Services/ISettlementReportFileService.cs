using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports.Services;

public interface ISettlementReportFileService
{
    Task<Stream> DownloadAsync(ReportRequestId requestId, Guid actorId, bool isMultitenancy);
}
