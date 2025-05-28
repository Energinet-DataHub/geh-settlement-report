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

using Energinet.DataHub.Reports.Application.Model;
using Energinet.DataHub.Reports.Application.Services;
using Energinet.DataHub.Reports.Infrastructure.Contracts;
using Energinet.DataHub.Reports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Reports.Infrastructure.Services;

public sealed class GridAreaOwnerRepository : IGridAreaOwnerRepository, IGridAreaOwnershipAssignedEventStore
{
    private readonly ISettlementReportDatabaseContext _dbContext;

    public GridAreaOwnerRepository(ISettlementReportDatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<GridAreaOwner>> GetGridAreaOwnersAsync(GridAreaCode gridAreaCode, Instant periodFrom, Instant periodTo)
    {
        ArgumentNullException.ThrowIfNull(gridAreaCode);

        var f = periodFrom.ToDateTimeOffset();
        var t = periodTo.ToDateTimeOffset();

        var inner = from ga in _dbContext.GridAreaOwners
            select new
            {
                ga.Code,
                ga.ActorNumber,
                ga.ValidFrom,
                ValidTo = _dbContext.GridAreaOwners
                    .Where(x => x.Code == ga.Code && x.ValidFrom > ga.ValidFrom)
                    .Select(x => (DateTimeOffset?)x.ValidFrom)
                    .OrderBy(x => x).FirstOrDefault(),
                ga.SequenceNumber,
            };

        var query = from ga in inner
            where
                ga.Code == gridAreaCode.Value &&
                (ga.ValidTo == null || f <= ga.ValidTo) && ga.ValidFrom < t
            orderby ga.ValidFrom descending, ga.SequenceNumber descending
            select new
            {
                ga.Code,
                ga.ActorNumber,
                ga.ValidFrom,
            };

        return (await query.ToListAsync().ConfigureAwait(false))
            .Select(x => new GridAreaOwner(new GridAreaCode(x.Code), new ActorNumber(x.ActorNumber), Instant.FromDateTimeOffset(x.ValidFrom)));
    }

    public async Task AddAsync(GridAreaOwnershipAssigned gridAreaOwnershipAssigned)
    {
        ArgumentNullException.ThrowIfNull(gridAreaOwnershipAssigned);

        var entity = new GridAreaOwnerEntity
        {
            Code = gridAreaOwnershipAssigned.GridAreaCode,
            ActorNumber = gridAreaOwnershipAssigned.ActorNumber,
            ValidFrom = gridAreaOwnershipAssigned.ValidFrom.ToDateTimeOffset(),
            SequenceNumber = gridAreaOwnershipAssigned.SequenceNumber,
        };

        await _dbContext.GridAreaOwners.AddAsync(entity).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
