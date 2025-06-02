using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;

public sealed class MeasurementsReportRepository : IMeasurementsReportRepository
{
    private readonly ISettlementReportDatabaseContext _context;

    public MeasurementsReportRepository(ISettlementReportDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(Application.SettlementReports_v2.MeasurementsReport measurementsReport)
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

    public Task<Application.SettlementReports_v2.MeasurementsReport> GetByRequestIdAsync(string requestId)
    {
        return _context.MeasurementsReports.FirstAsync(x => x.RequestId == requestId);
    }

    public async Task<IEnumerable<Application.SettlementReports_v2.MeasurementsReport>> GetByActorIdAsync(Guid actorId)
    {
        return await _context.MeasurementsReports
             .Where(x => x.ActorId == actorId && x.JobRunId == null)
             .OrderByDescending(x => x.Id)
             .ToListAsync()
             .ConfigureAwait(false);
    }

    public Task<Application.SettlementReports_v2.MeasurementsReport> GetByJobRunIdAsync(long jobRunId)
    {
        throw new NotImplementedException();
    }
}
