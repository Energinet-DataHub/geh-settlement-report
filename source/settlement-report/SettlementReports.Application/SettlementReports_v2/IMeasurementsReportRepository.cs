namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public interface IMeasurementsReportRepository
{
    Task<MeasurementsReport> GetAsync(string requestId);
}
