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

using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Persistence.SettlementReportRequest;

public sealed class ReportRepository<T> : IReportRepository<T>
    where T : Report
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public ReportRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task AddOrUpdateAsync(T report)
    {
        if (report.Id == 0)
        {
            await _dbSet.AddAsync(report).ConfigureAwait(false);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(T request)
    {
        _dbSet.Remove(request);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public Task<IEnumerable<T>> GetByActorIdAsync(Guid actorId)
    {
        throw new NotImplementedException();
    }
}
