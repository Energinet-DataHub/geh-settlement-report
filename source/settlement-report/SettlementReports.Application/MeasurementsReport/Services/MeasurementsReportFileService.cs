using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Application.Services;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public sealed class MeasurementsReportFileService : IMeasurementsReportFileService
{
    private readonly IMeasurementsReportFileRepository _measurementsReportFileRepository;
    private readonly IMeasurementsReportRepository _repository;

    public MeasurementsReportFileService(
        IMeasurementsReportFileRepository measurementsReportFileRepository,
        IMeasurementsReportRepository repository)
    {
        _measurementsReportFileRepository = measurementsReportFileRepository;
        _repository = repository;
    }

    public async Task<Stream> DownloadAsync(ReportRequestId requestId)
    {
        var report = await _repository.GetByRequestIdAsync(requestId.Id).ConfigureAwait(false);

        if (string.IsNullOrEmpty(report.BlobFileName))
            throw new InvalidOperationException("Report does not have a blob file name.");

        return await _measurementsReportFileRepository
            .DownloadAsync(report.BlobFileName)
            .ConfigureAwait(false);
    }
}
