using Energinet.DataHub.Reports.Interfaces.Models;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Commands;

public sealed record CancelSettlementReportCommand(
    ReportRequestId RequestId,
    Guid UserId);
