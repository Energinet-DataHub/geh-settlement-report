using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Services;

public interface ISettlementReportFileService
{
    Task<Stream> DownloadAsync(ReportRequestId requestId, Guid actorId, bool isMultitenancy);
}
