using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models;

namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

public record JobRunWithStatusAndEndTime(JobRunStatus Status, DateTimeOffset? EndTime);
