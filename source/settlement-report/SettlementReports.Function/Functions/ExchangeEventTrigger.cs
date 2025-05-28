using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Energinet.DataHub.Reports.Common.Infrastructure.Telemetry;
using Energinet.DataHub.RevisionLog.Integration;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace Energinet.DataHub.Reports.Function.Functions;

/// <summary>
/// They are used to update "grid-area-ownership-assigned" so that settlement reports know the current,
/// but also potentially previous, owners — since this is used to send correct data to Databricks in case
/// someone is a new owner and wants to generate a report retrospectively.
/// </summary>
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
