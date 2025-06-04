using Energinet.DataHub.Reports.Application.Model;
using Energinet.DataHub.Reports.Application.Services;
using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public sealed class MeasurementsReportFileService : IMeasurementsReportFileService
{
    private readonly ISettlementReportFileRepository _settlementReportFileRepository;
    private readonly IMeasurementsReportRepository _repository;

    public MeasurementsReportFileService(
        ISettlementReportFileRepository settlementReportFileRepository,
        IMeasurementsReportRepository repository)
    {
        _settlementReportFileRepository = settlementReportFileRepository;
        _repository = repository;
    }

    public async Task<Stream> DownloadAsync(ReportRequestId requestId)
    {
        var report = await _repository.GetByRequestIdAsync(requestId.Id).ConfigureAwait(false);

        if (string.IsNullOrEmpty(report.BlobFileName))
            throw new InvalidOperationException("Report does not have a blob file name.");

        return await _settlementReportFileRepository
            .DownloadAsync(report.BlobFileName)
            .ConfigureAwait(false);
    }
}
