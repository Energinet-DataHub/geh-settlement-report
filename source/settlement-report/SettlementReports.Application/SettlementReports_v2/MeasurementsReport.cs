using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public sealed class MeasurementsReport
{
    public MeasurementsReport(string? blobFileName = null)
    {
        BlobFileName = blobFileName;
    }

    public MeasurementsReport(
        IClock clock,
        Guid userId,
        Guid actorId,
        ReportRequestId requestId,
        MeasurementsReportRequestDto request)
    {
        RequestId = requestId.Id;
        UserId = userId;
        ActorId = actorId;
        CreatedDateTime = clock.GetCurrentInstant();
        Status = ReportStatus.InProgress;
        PeriodStart = request.Filter.PeriodStart.ToInstant();
        PeriodEnd = request.Filter.PeriodEnd.ToInstant();
        GridAreaCodes = request.Filter.GridAreaCodes.ToList();
    }

    // EF Core Constructor.
    // ReSharper disable once UnusedMember.Local
    private MeasurementsReport()
    {
    }

    /// <summary>
    ///     Internal (database) ID of the report.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    ///     The public ID of the report.
    /// </summary>
    public string RequestId { get; init; } = null!;

    public Guid ActorId { get; init; }

    public Guid UserId { get; init; }

    public Instant PeriodStart { get; }

    public Instant PeriodEnd { get; }

    public Instant CreatedDateTime { get; init; }

    public Instant? EndedDateTime { get; private set; }

    public ReportStatus Status { get; private set; }

    public string? BlobFileName { get; private set; }

    /// <summary>
    ///     The Databricks job run ID of the job run creating the report.
    /// </summary>
    public long? JobRunId { get; init; }

    public IReadOnlyList<string> GridAreaCodes { get; init; } = null!;

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
