using Energinet.DataHub.Reports.Application.Services;
using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Interfaces.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Reports.Infrastructure.Services;

public sealed class RemoveExpiredSettlementReports : IRemoveExpiredSettlementReports
{
    private readonly IClock _clock;
    private readonly ISettlementReportFileRepository _settlementReportFileRepository;
    private readonly ISettlementReportRepository _settlementReportRepository;

    public RemoveExpiredSettlementReports(
        IClock clock,
        ISettlementReportRepository settlementReportRepository,
        ISettlementReportFileRepository settlementReportFileRepository)
    {
        _clock = clock;
        _settlementReportRepository = settlementReportRepository;
        _settlementReportFileRepository = settlementReportFileRepository;
    }

    public async Task RemoveExpiredAsync(IList<SettlementReport> settlementReports)
    {
        for (var i = 0; i < settlementReports.Count; i++)
        {
            var settlementReport = settlementReports[i];

            if (!IsExpired(settlementReport))
            {
                continue;
            }

            // Delete the blob file if it exists and the job id is null, because on the shared blob storage we use retention to clean-up
            if (settlementReport is { BlobFileName: not null, JobId: null })
            {
                await _settlementReportFileRepository
                    .DeleteAsync(
                        new ReportRequestId(settlementReport.RequestId),
                        settlementReport.BlobFileName)
                    .ConfigureAwait(false);
            }

            await _settlementReportRepository
                .DeleteAsync(settlementReport)
                .ConfigureAwait(false);

            settlementReports.RemoveAt(i--);
        }
    }

    private bool IsExpired(SettlementReport settlementReport)
    {
        var cutOffPeriod = _clock
            .GetCurrentInstant()
            .Minus(TimeSpan.FromDays(7).ToDuration());

        return settlementReport.Status != ReportStatus.InProgress &&
               settlementReport.CreatedDateTime <= cutOffPeriod;
    }
}
