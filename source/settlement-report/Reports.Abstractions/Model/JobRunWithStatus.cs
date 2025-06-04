namespace Energinet.DataHub.Reports.Abstractions.Model;

public record JobRunWithStatusAndEndTime(JobRunStatus Status, DateTimeOffset? EndTime);
