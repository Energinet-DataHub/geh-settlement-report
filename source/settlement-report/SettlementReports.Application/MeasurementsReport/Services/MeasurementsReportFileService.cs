using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;

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
        var report = await _repository.GetAsync(requestId.Id).ConfigureAwait(false);

        if (string.IsNullOrEmpty(report.BlobFileName))
            throw new InvalidOperationException("Report does not have a blob file name.");

        // TODO BJM: Replace dummy implementation when stories #784 and #759 are completed
        return new MemoryStream();
        // return await _reportFileRepository
        //     .DownloadAsync(ReportType.Measurements, report.BlobFileName)
        //     .ConfigureAwait(false);
    }
}
