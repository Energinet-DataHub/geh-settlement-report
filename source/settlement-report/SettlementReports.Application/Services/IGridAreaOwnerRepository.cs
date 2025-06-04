using Energinet.DataHub.Reports.Application.Model;
using NodaTime;

namespace Energinet.DataHub.Reports.Application.Services;

public interface IGridAreaOwnerRepository
{
    Task<IEnumerable<GridAreaOwner>> GetGridAreaOwnersAsync(GridAreaCode gridAreaCode, Instant periodFrom, Instant periodTo);
}
