namespace Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;

public record JobRunWithStatusAndEndTime(JobRunStatus Status, DateTimeOffset? EndTime);
