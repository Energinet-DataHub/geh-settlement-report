using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReportRequest;

public sealed class SettlementReportRepository : ISettlementReportRepository
{
    private readonly ISettlementReportDatabaseContext _context;

    public SettlementReportRepository(ISettlementReportDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(SettlementReport request)
    {
        if (request.Id == 0)
        {
            await _context.SettlementReports.AddAsync(request).ConfigureAwait(false);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(SettlementReport request)
    {
        _context.SettlementReports.Remove(request);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public Task<SettlementReport> GetAsync(string requestId)
    {
        return _context.SettlementReports
            .FirstAsync(x => x.RequestId == requestId);
    }

    public async Task<IEnumerable<SettlementReport>> GetAsync()
    {
        return await _context.SettlementReports
            .Where(x => x.JobId == null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<SettlementReport>> GetAsync(Guid actorId)
    {
        return await _context.SettlementReports
            .Where(x => x.ActorId == actorId && !x.IsHiddenFromActor && x.JobId == null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public Task<SettlementReport> GetAsync(long jobId)
    {
        return _context.SettlementReports
            .FirstAsync(x => x.JobId == jobId);
    }

    public async Task<IEnumerable<SettlementReport>> GetForJobsAsync()
    {
        return await _context.SettlementReports
            .Where(x => x.JobId != null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<SettlementReport>> GetForJobsAsync(Guid actorId)
    {
        return await _context.SettlementReports
            .Where(x => x.ActorId == actorId && !x.IsHiddenFromActor && x.JobId != null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<SettlementReport>> GetPendingNotificationsForCompletedAndFailed()
    {
        return await _context.SettlementReports
            .Where(x => x.IsNotificationSent == false &&
                        (x.Status == ReportStatus.Completed || x.Status == ReportStatus.Failed))
            .OrderBy(x => x.EndedDateTime)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
