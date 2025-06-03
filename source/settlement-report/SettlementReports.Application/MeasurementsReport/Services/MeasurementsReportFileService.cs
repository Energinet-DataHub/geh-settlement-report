using Energinet.DataHub.Reports.Application.Model;
using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public sealed class MeasurementsReportFileService : IMeasurementsReportFileService
{
    private readonly IReportFileRepository _reportFileRepository;
    private readonly IMeasurementsReportRepository _repository;

    public MeasurementsReportFileService(
        IReportFileRepository reportFileRepository,
        IMeasurementsReportRepository repository)
    {
        _reportFileRepository = reportFileRepository;
        _repository = repository;
    }

    public async Task<Stream> DownloadAsync(ReportRequestId requestId)
    {
        var report = await _repository.GetByRequestIdAsync(requestId.Id).ConfigureAwait(false);

        if (string.IsNullOrEmpty(report.BlobFileName))
            throw new InvalidOperationException("Report does not have a blob file name.");

        return await _reportFileRepository
            .DownloadAsync(ReportType.Measurements, report.BlobFileName)
            .ConfigureAwait(false);
    }
}
