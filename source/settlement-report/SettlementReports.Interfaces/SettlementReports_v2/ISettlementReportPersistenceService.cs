using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.Reports.Interfaces.SettlementReports_v2;

/// <summary>
/// A thin service that instantiates the persistence model and calls the repository.
/// </summary>
public interface ISettlementReportPersistenceService
{
    Task PersistAsync(
        Guid userId,
        Guid actorId,
        bool hideReport,
        JobRunId jobId,
        ReportRequestId requestId,
        SettlementReportRequestDto request);
}
