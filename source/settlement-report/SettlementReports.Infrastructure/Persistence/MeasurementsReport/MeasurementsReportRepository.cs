using Energinet.DataHub.Reports.Application.MeasurementsReport;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;

public sealed class MeasurementsReportRepository : IMeasurementsReportRepository
{
    private readonly IReportsDatabaseContext _context;

    public MeasurementsReportRepository(IReportsDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(Application.MeasurementsReport.MeasurementsReport measurementsReport)
    {
        if (measurementsReport.Id == 0)
        {
            await _context.MeasurementsReports
                .AddAsync(measurementsReport)
                .ConfigureAwait(false);
        }

        await _context
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public Task<Application.MeasurementsReport.MeasurementsReport> GetByRequestIdAsync(string requestId)
    {
        return _context.MeasurementsReports.FirstAsync(x => x.RequestId == requestId);
    }

    public async Task<IEnumerable<Application.MeasurementsReport.MeasurementsReport>> GetByActorIdAsync(Guid actorId)
    {
        return await _context.MeasurementsReports
             .Where(x => x.ActorId == actorId && x.JobRunId != null)
             .OrderByDescending(x => x.Id)
             .ToListAsync()
             .ConfigureAwait(false);
    }

    public Task<Application.MeasurementsReport.MeasurementsReport> GetByJobRunIdAsync(long jobRunId)
    {
        return _context.MeasurementsReports.FirstAsync(x => x.JobRunId == jobRunId);
    }
}
