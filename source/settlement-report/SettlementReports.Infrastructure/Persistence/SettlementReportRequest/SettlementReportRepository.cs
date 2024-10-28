// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Persistence.SettlementReportRequest;

public sealed class SettlementReportRepository : ISettlementReportRepository
{
    private readonly ISettlementReportDatabaseContext _context;

    public SettlementReportRepository(ISettlementReportDatabaseContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(Application.SettlementReports_v2.SettlementReport request)
    {
        if (request.Id == 0)
        {
            await _context.SettlementReports.AddAsync(request).ConfigureAwait(false);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(Application.SettlementReports_v2.SettlementReport request)
    {
        _context.SettlementReports.Remove(request);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public Task<Application.SettlementReports_v2.SettlementReport> GetAsync(string requestId)
    {
        return _context.SettlementReports
            .FirstAsync(x => x.RequestId == requestId);
    }

    public async Task<IEnumerable<Application.SettlementReports_v2.SettlementReport>> GetAsync()
    {
        return await _context.SettlementReports
            .Where(x => x.JobId == null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Application.SettlementReports_v2.SettlementReport>> GetAsync(Guid actorId)
    {
        return await _context.SettlementReports
            .Where(x => x.ActorId == actorId && !x.IsHiddenFromActor && x.JobId == null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public Task<Application.SettlementReports_v2.SettlementReport> GetAsync(long jobId)
    {
        return _context.SettlementReports
            .FirstAsync(x => x.JobId == jobId);
    }

    public async Task<IEnumerable<Application.SettlementReports_v2.SettlementReport>> GetForJobsAsync()
    {
        return await _context.SettlementReports
            .Where(x => x.JobId != null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Application.SettlementReports_v2.SettlementReport>> GetForJobsAsync(Guid actorId)
    {
        return await _context.SettlementReports
            .Where(x => x.ActorId == actorId && !x.IsHiddenFromActor && x.JobId != null)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Application.SettlementReports_v2.SettlementReport>> GetNeedsNotificationSent()
    {
        return await _context.SettlementReports
            .Where(x => x.IsNotficationSent == false)
            .OrderBy(x => x.EndedDateTime)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
