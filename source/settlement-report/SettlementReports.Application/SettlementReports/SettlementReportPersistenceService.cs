﻿using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;
using NodaTime;

namespace Energinet.DataHub.Reports.Application.SettlementReports;

public sealed class SettlementReportPersistenceService : ISettlementReportPersistenceService
{
    private readonly ISettlementReportRepository _repository;

    public SettlementReportPersistenceService(ISettlementReportRepository repository)
    {
        _repository = repository;
    }

    public Task PersistAsync(
        Guid userId,
        Guid actorId,
        bool hideReport,
        JobRunId jobId,
        ReportRequestId requestId,
        SettlementReportRequestDto request)
    {
        var settlementReport = new SettlementReport(SystemClock.Instance, userId, actorId, hideReport, jobId, requestId, request);
        return _repository.AddOrUpdateAsync(settlementReport);
    }
}
