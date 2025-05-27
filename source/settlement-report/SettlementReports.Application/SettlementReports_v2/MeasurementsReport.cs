using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public sealed class MeasurementsReport
{
    public Instant? EndedDateTime { get; private set; }

    public ReportStatus Status { get; private set; }

    public MeasurementsReport(string? blobFileName = null)
    {
        BlobFileName = blobFileName;
    }

    public string? BlobFileName { get; private set; }

    public void MarkAsCompleted(IClock clock, ReportRequestId requestId, DateTimeOffset? endTime)
    {
        Status = ReportStatus.Completed;
        BlobFileName = requestId.Id + ".zip";
        EndedDateTime = endTime?.ToInstant() ?? clock.GetCurrentInstant();
    }

    public void MarkAsFailed()
    {
        Status = ReportStatus.Failed;
    }

    public void MarkAsCanceled()
    {
        Status = ReportStatus.Canceled;
    }
}
