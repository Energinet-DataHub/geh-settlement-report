using Energinet.DataHub.Reports.Abstractions.Model;

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

    public async Task<Stream> DownloadAsync(ReportRequestId requestId, Guid actorId)
    {
        var report = await _repository.GetByRequestIdAsync(requestId.Id).ConfigureAwait(false);

        if (report.ActorId != actorId)
            throw new InvalidOperationException("User does not have access to the report.");

        if (string.IsNullOrEmpty(report.BlobFileName))
            throw new InvalidOperationException("Report does not have a blob file name.");

        return await _measurementsReportFileRepository
            .DownloadAsync(report.BlobFileName)
            .ConfigureAwait(false);
    }
}
