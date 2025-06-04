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
