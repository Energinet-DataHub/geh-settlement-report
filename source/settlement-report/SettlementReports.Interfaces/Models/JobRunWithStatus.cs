namespace Energinet.DataHub.Reports.Interfaces.Models;

public record JobRunWithStatusAndEndTime(JobRunStatus Status, DateTimeOffset? EndTime);
