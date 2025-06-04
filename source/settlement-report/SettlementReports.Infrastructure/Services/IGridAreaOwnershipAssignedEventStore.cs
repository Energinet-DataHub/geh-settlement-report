using Energinet.DataHub.Reports.Infrastructure.Contracts;

namespace Energinet.DataHub.Reports.Infrastructure.Services;

public interface IGridAreaOwnershipAssignedEventStore
{
    Task AddAsync(GridAreaOwnershipAssigned gridAreaOwnershipAssigned);
}
