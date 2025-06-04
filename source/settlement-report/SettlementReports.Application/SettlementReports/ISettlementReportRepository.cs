namespace Energinet.DataHub.Reports.Application.SettlementReports;

public interface ISettlementReportRepository
{
    Task AddOrUpdateAsync(SettlementReport request);

    Task DeleteAsync(SettlementReport request);

    Task<SettlementReport> GetAsync(string requestId);

    Task<IEnumerable<SettlementReport>> GetAsync();

    Task<IEnumerable<SettlementReport>> GetAsync(Guid actorId);

    Task<SettlementReport> GetAsync(long jobId);

    Task<IEnumerable<SettlementReport>> GetForJobsAsync();

    Task<IEnumerable<SettlementReport>> GetForJobsAsync(Guid actorId);

    Task<IEnumerable<SettlementReport>> GetPendingNotificationsForCompletedAndFailed();
}
