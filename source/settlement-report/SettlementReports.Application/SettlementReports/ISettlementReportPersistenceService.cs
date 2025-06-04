using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

namespace Energinet.DataHub.Reports.Application.SettlementReports;

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
