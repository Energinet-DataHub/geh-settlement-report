using Energinet.DataHub.Reports.Application.SettlementReports.Commands;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Handlers;

public interface ICancelSettlementReportJobHandler
{
    Task HandleAsync(CancelSettlementReportCommand cancelSettlementReportCommand);
}
