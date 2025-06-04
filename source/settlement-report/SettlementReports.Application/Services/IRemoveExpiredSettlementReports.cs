using Energinet.DataHub.Reports.Application.SettlementReports;

namespace Energinet.DataHub.Reports.Application.Services;

public interface IRemoveExpiredSettlementReports
{
    Task RemoveExpiredAsync(IList<SettlementReport> settlementReports);
}
