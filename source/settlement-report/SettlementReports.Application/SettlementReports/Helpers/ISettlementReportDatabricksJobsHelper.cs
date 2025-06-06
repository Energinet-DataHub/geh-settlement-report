﻿using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

namespace Energinet.DataHub.Reports.Infrastructure.Helpers;

public interface ISettlementReportDatabricksJobsHelper
{
    Task<JobRunId> RunJobAsync(
        SettlementReportRequestDto request,
        MarketRole marketRole,
        ReportRequestId reportRequestId,
        string actorGln);

    Task<JobRunWithStatusAndEndTime> GetJobRunAsync(long jobRunId);

    Task CancelAsync(long jobRunId);
}
