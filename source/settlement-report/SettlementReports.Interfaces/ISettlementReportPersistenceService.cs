using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

namespace Energinet.DataHub.Reports.Interfaces;

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
