using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Interfaces.Models;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReport;

public sealed class SettlementReportRepository : ISettlementReportRepository
{
    private readonly IReportsDatabaseContext _context;

    public SettlementReportRepository(IReportsDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(Application.SettlementReports.SettlementReport request)
    {
        if (request.Id == 0)
        {
            await _context.SettlementReports.AddAsync(request).ConfigureAwait(false);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(Application.SettlementReports.SettlementReport request)
    {
        _context.SettlementReports.Remove(request);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public Task<Application.SettlementReports.SettlementReport> GetAsync(string requestId)
    {
        return _context.SettlementReports
            .FirstAsync(x => x.RequestId == requestId);
    }

    public async Task<IEnumerable<Application.SettlementReports.SettlementReport>> GetAsync()
    {
        return await _context.SettlementReports
            .Where(x => x.JobId == null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Application.SettlementReports.SettlementReport>> GetAsync(Guid actorId)
    {
        return await _context.SettlementReports
            .Where(x => x.ActorId == actorId && !x.IsHiddenFromActor && x.JobId == null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public Task<Application.SettlementReports.SettlementReport> GetAsync(long jobId)
    {
        return _context.SettlementReports
            .FirstAsync(x => x.JobId == jobId);
    }

    public async Task<IEnumerable<Application.SettlementReports.SettlementReport>> GetForJobsAsync()
    {
        return await _context.SettlementReports
            .Where(x => x.JobId != null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Application.SettlementReports.SettlementReport>> GetForJobsAsync(Guid actorId)
    {
        return await _context.SettlementReports
            .Where(x => x.ActorId == actorId && !x.IsHiddenFromActor && x.JobId != null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Application.SettlementReports.SettlementReport>> GetPendingNotificationsForCompletedAndFailed()
    {
        return await _context.SettlementReports
            .Where(x => x.IsNotificationSent == false &&
                        (x.Status == ReportStatus.Completed || x.Status == ReportStatus.Failed))
            .OrderBy(x => x.EndedDateTime)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
