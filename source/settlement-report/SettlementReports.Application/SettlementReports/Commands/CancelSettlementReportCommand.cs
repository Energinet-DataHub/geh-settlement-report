using Energinet.DataHub.Reports.Abstractions.Model;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Commands;

public sealed record CancelSettlementReportCommand(
    ReportRequestId RequestId,
    Guid UserId);
