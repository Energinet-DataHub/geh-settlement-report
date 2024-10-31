// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text.Json;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public sealed class SettlementReport
{
    public int Id { get; init; }

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

    public SettlementReportStatus Status { get; private set; }

    public string? BlobFileName { get; private set; }

    public long? JobId { get; init; }

    public SettlementReport(
        IClock clock,
        Guid userId,
        Guid actorId,
        bool hideReport,
        SettlementReportRequestId requestId,
        SettlementReportRequestDto request)
    {
        RequestId = requestId.Id;
        UserId = userId;
        ActorId = actorId;
        IsHiddenFromActor = hideReport;
        CreatedDateTime = clock.GetCurrentInstant();
        Status = SettlementReportStatus.InProgress;
        CalculationType = request.Filter.CalculationType;
        ContainsBasisData = request.IncludeBasisData;
        PeriodStart = request.Filter.PeriodStart.ToInstant();
        PeriodEnd = request.Filter.PeriodEnd.ToInstant();
        GridAreaCount = request.Filter.GridAreas.Count;
        SplitReportPerGridArea = request.SplitReportPerGridArea;
        IncludeMonthlyAmount = request.IncludeMonthlyAmount;
        GridAreas = JsonSerializer.Serialize(request.Filter.GridAreas);
    }

    public SettlementReport(
        IClock clock,
        Guid userId,
        Guid actorId,
        bool hideReport,
        JobRunId jobRunId,
        SettlementReportRequestId requestId,
        SettlementReportRequestDto request)
    {
        RequestId = requestId.Id;
        JobId = jobRunId.Id;
        UserId = userId;
        ActorId = actorId;
        IsHiddenFromActor = hideReport;
        CreatedDateTime = clock.GetCurrentInstant();
        Status = SettlementReportStatus.InProgress;
        CalculationType = request.Filter.CalculationType;
        ContainsBasisData = request.IncludeBasisData;
        PeriodStart = request.Filter.PeriodStart.ToInstant();
        PeriodEnd = request.Filter.PeriodEnd.ToInstant();
        GridAreaCount = request.Filter.GridAreas.Count;
        SplitReportPerGridArea = request.SplitReportPerGridArea;
        IncludeMonthlyAmount = request.IncludeMonthlyAmount;
        GridAreas = JsonSerializer.Serialize(request.Filter.GridAreas);
    }

    // EF Core Constructor.
    // ReSharper disable once UnusedMember.Local
    private SettlementReport()
    {
    }

    public void MarkAsCompleted(IClock clock, GeneratedSettlementReportDto generatedSettlementReport)
    {
        Status = SettlementReportStatus.Completed;
        BlobFileName = generatedSettlementReport.ReportFileName;
        EndedDateTime = clock.GetCurrentInstant();
    }

    public void MarkAsCompleted(IClock clock, SettlementReportRequestId requestId)
    {
        Status = SettlementReportStatus.Completed;
        BlobFileName = requestId.Id + ".zip";
        EndedDateTime = clock.GetCurrentInstant();
    }

    public void MarkAsFailed()
    {
        Status = SettlementReportStatus.Failed;
    }

    public void MarkAsCanceled()
    {
        Status = SettlementReportStatus.Canceled;
    }
}
