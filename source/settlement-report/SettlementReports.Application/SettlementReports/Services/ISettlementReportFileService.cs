﻿using Energinet.DataHub.Reports.Abstractions.Model;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Services;

public interface ISettlementReportFileService
{
    Task<Stream> DownloadAsync(ReportRequestId requestId, Guid actorId, bool isMultitenancy);
}
