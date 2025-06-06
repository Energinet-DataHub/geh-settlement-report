﻿using System.Text.Json;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Reports.Application.SettlementReports;

public sealed class SettlementReport
{
    /// <summary>
    /// Internal (database) ID of the report.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The public ID of the report.
    /// </summary>
    public string RequestId { get; init; } = null!;

    public Guid UserId { get; init; }

    public Guid ActorId { get; init; }

    public Instant CreatedDateTime { get; init; }

    public Instant? EndedDateTime { get; private set; }

    public CalculationType CalculationType { get; init; }

    public bool ContainsBasisData { get; init; }

    public bool IsHiddenFromActor { get; init; }

    public Instant PeriodStart { get; init; }

    public Instant PeriodEnd { get; init; }

    public int GridAreaCount { get; init; }

    public bool SplitReportPerGridArea { get; init; }

    public bool IncludeMonthlyAmount { get; init; }

    public string GridAreas { get; init; } = null!;

    public ReportStatus Status { get; private set; }

    public string? BlobFileName { get; private set; }

    /// <summary>
    /// The Databricks job run ID of the job run creating the report.
    /// </summary>
    public long? JobId { get; init; }

    public bool IsNotificationSent { get; private set; }

    public SettlementReport(
        IClock clock,
        Guid userId,
        Guid actorId,
        bool hideReport,
        ReportRequestId requestId,
        SettlementReportRequestDto request)
    {
        RequestId = requestId.Id;
        UserId = userId;
        ActorId = actorId;
        IsHiddenFromActor = hideReport;
        CreatedDateTime = clock.GetCurrentInstant();
        Status = ReportStatus.InProgress;
        CalculationType = request.Filter.CalculationType;
        ContainsBasisData = request.IncludeBasisData;
        PeriodStart = request.Filter.PeriodStart.ToInstant();
        PeriodEnd = request.Filter.PeriodEnd.ToInstant();
        GridAreaCount = request.Filter.GridAreas.Count;
        SplitReportPerGridArea = request.SplitReportPerGridArea;
        IncludeMonthlyAmount = request.IncludeMonthlyAmount;
        GridAreas = JsonSerializer.Serialize(request.Filter.GridAreas);
        IsNotificationSent = false;
    }

    public SettlementReport(
        IClock clock,
        Guid userId,
        Guid actorId,
        bool hideReport,
        JobRunId jobRunId,
        ReportRequestId requestId,
        SettlementReportRequestDto request)
    {
        RequestId = requestId.Id;
        JobId = jobRunId.Id;
        UserId = userId;
        ActorId = actorId;
        IsHiddenFromActor = hideReport;
        CreatedDateTime = clock.GetCurrentInstant();
        Status = ReportStatus.InProgress;
        CalculationType = request.Filter.CalculationType;
        ContainsBasisData = request.IncludeBasisData;
        PeriodStart = request.Filter.PeriodStart.ToInstant();
        PeriodEnd = request.Filter.PeriodEnd.ToInstant();
        GridAreaCount = request.Filter.GridAreas.Count;
        SplitReportPerGridArea = request.SplitReportPerGridArea;
        IncludeMonthlyAmount = request.IncludeMonthlyAmount;
        GridAreas = JsonSerializer.Serialize(request.Filter.GridAreas);
        IsNotificationSent = false;
    }

    // EF Core Constructor.
    // ReSharper disable once UnusedMember.Local
    private SettlementReport()
    {
    }

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

    public void MarkAsNotificationSent()
    {
        IsNotificationSent = true;
    }

    public void MarkAsCanceled()
    {
        Status = ReportStatus.Canceled;
    }
}
