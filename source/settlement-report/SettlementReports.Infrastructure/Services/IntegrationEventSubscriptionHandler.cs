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
