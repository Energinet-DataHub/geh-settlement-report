using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Application.Model;
using Energinet.DataHub.Reports.Application.Services;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Services;

public sealed class SettlementReportFileService : ISettlementReportFileService
{
    private readonly ISettlementReportFileRepository _settlementReportFileRepository;
    private readonly ISettlementReportRepository _repository;

    public SettlementReportFileService(
        ISettlementReportFileRepository settlementReportFileRepository,
        ISettlementReportRepository repository)
    {
        _settlementReportFileRepository = settlementReportFileRepository;
        _repository = repository;
    }

    public async Task<Stream> DownloadAsync(
        ReportRequestId requestId,
        Guid actorId,
        bool isMultitenancy)
    {
        var report = await _repository
            .GetAsync(requestId.Id)
            .ConfigureAwait(false) ?? throw new InvalidOperationException("Report not found.");

        if (!isMultitenancy && (report.ActorId != actorId || report.IsHiddenFromActor))
        {
            throw new InvalidOperationException("User does not have access to the report.");
        }

        if (string.IsNullOrEmpty(report.BlobFileName))
            throw new InvalidOperationException("Report does not have a Blob file name.");

        return await _settlementReportFileRepository
            .DownloadAsync(report.BlobFileName)
            .ConfigureAwait(false);
    }
}
