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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Energinet.DataHub.RevisionLog.Integration;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Telemetry;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace SettlementReports.Function.Functions;

public sealed class ExchangeEventTrigger
{
    private readonly ISubscriber _subscriber;
    private readonly IRevisionLogClient _revisionLogClient;

    public ExchangeEventTrigger(ISubscriber subscriber, IRevisionLogClient revisionLogClient)
    {
        _subscriber = subscriber;
        _revisionLogClient = revisionLogClient;
    }

    [Function(nameof(ExchangeEventTrigger))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            $"%{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.TopicName)}%",
            $"%{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.SubscriptionName)}%",
            Connection = ServiceBusNamespaceOptions.SectionName)]
        ServiceBusReceivedMessage message,
        FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);

        var msg = IntegrationEventServiceBusMessage.Create(message);

        await _revisionLogClient.LogAsync(
                new RevisionLogEntry(
                    logId: msg.MessageId,
                    systemId: SubsystemInformation.Id,
                    occurredOn: SystemClock.Instance.GetCurrentInstant(),
                    activity: "ConsumedIntegrationEvent",
                    origin: nameof(ExchangeEventTrigger),
                    payload: Convert.ToBase64String(message.Body)))
            .ConfigureAwait(false);

        await _subscriber.HandleAsync(msg).ConfigureAwait(false);
    }
}
