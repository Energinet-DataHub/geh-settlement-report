using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Energinet.DataHub.Reports.Infrastructure.Contracts;
using GridAreaOwnershipAssigned = Energinet.DataHub.Reports.Infrastructure.Contracts.GridAreaOwnershipAssigned;

namespace Energinet.DataHub.Reports.Infrastructure.Services;

public sealed class IntegrationEventSubscriptionHandler : IIntegrationEventHandler
{
    private readonly IGridAreaOwnershipAssignedEventStore _areaOwnershipAssignedEventStore;

    public IntegrationEventSubscriptionHandler(IGridAreaOwnershipAssignedEventStore areaOwnershipAssignedEventStore)
    {
        _areaOwnershipAssignedEventStore = areaOwnershipAssignedEventStore;
    }

    public Task HandleAsync(IntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        switch (integrationEvent.Message)
        {
            case GridAreaOwnershipAssigned { ActorRole: EicFunction.GridAccessProvider } gridAreaOwnershipAssigned:
                return _areaOwnershipAssignedEventStore.AddAsync(gridAreaOwnershipAssigned);

            default:
                return Task.CompletedTask;
        }
    }
}
