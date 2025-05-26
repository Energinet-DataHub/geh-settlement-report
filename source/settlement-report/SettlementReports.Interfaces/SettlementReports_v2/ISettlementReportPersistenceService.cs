using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;

/// <summary>
/// A thin service that instantiates the persistence model and calls the repository.
/// </summary>
public interface ISettlementReportPersistenceService
{
    Task PersistAsync(
        Guid userId,
        Guid actorId,
        bool hideReport,
        ReportRequestId requestId,
        SettlementReportRequestDto request);

    Task PersistAsync(
        Guid userId,
        Guid actorId,
        bool hideReport,
        JobRunId jobId,
        ReportRequestId requestId,
        SettlementReportRequestDto request);
}
