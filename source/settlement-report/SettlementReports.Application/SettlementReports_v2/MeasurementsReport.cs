using System.Text.Json;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Reports.Application.SettlementReports_v2;

public sealed class MeasurementsReport
{
    public MeasurementsReport(string? blobFileName = null)
    {
        BlobFileName = blobFileName;
        GridAreaCodes = "test";
    }

    public MeasurementsReport(
        IClock clock,
        Guid userId,
        Guid actorId,
        bool isHiddenFromActor,
        ReportRequestId reportRequestId,
        MeasurementsReportRequestDto request)
    {
        RequestId = reportRequestId.Id;
        UserId = userId;
        ActorId = actorId;
        IsHiddenFromActor = isHiddenFromActor;
        CreatedDateTime = clock.GetCurrentInstant();
        Status = ReportStatus.InProgress;
        PeriodStart = request.Filter.PeriodStart.ToInstant();
        PeriodEnd = request.Filter.PeriodEnd.ToInstant();
        GridAreaCodes = JsonSerializer.Serialize(request.Filter.GridAreaCodes);
    }

    public MeasurementsReport(
        IClock clock,
        Guid userId,
        Guid actorId,
        bool isHiddenFromActor,
        JobRunId jobRunId,
        ReportRequestId reportRequestId,
        MeasurementsReportRequestDto request)
    {
        RequestId = reportRequestId.Id;
        UserId = userId;
        ActorId = actorId;
        IsHiddenFromActor = isHiddenFromActor;
        JobRunId = jobRunId.Id;
        CreatedDateTime = clock.GetCurrentInstant();
        Status = ReportStatus.InProgress;
        PeriodStart = request.Filter.PeriodStart.ToInstant();
        PeriodEnd = request.Filter.PeriodEnd.ToInstant();
        GridAreaCodes = JsonSerializer.Serialize(request.Filter.GridAreaCodes);
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

    /// <summary>
    /// If the report is requested by a system administrator on behalf of an actor (impersonation),
    /// then the actor must NOT access the report. The report is therefore hidden from the actor.
    /// </summary>
    public bool IsHiddenFromActor { get; init; }

    public Guid UserId { get; init; }

    public Instant PeriodStart { get; init; }

    public Instant PeriodEnd { get; init; }

    public Instant CreatedDateTime { get; init; }

    public Instant? EndedDateTime { get; private set; }

    public ReportStatus Status { get; private set; }

    public string? BlobFileName { get; private set; }

    /// <summary>
    ///     The Databricks job run ID of the job run creating the report.
    /// </summary>
    public long? JobRunId { get; init; }

    public string GridAreaCodes { get; init; } = null!;

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
