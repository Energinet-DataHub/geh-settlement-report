using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Infrastructure.Contracts;
using Energinet.DataHub.Reports.Interfaces.Models;
using Google.Protobuf.WellKnownTypes;

namespace Energinet.DataHub.Reports.Infrastructure.Notifications;

public sealed class IntegrationEventProvider : IIntegrationEventProvider
{
    private readonly ISettlementReportRepository _settlementReportRepository;

    public IntegrationEventProvider(ISettlementReportRepository settlementReportRepository)
    {
        _settlementReportRepository = settlementReportRepository;
    }

    public async IAsyncEnumerable<IntegrationEvent> GetAsync()
    {
        var reportsForNotifications = await _settlementReportRepository
            .GetPendingNotificationsForCompletedAndFailed()
            .ConfigureAwait(false);

        foreach (var reportForNotification in reportsForNotifications)
        {
            yield return await CreateAsync(reportForNotification, reportForNotification.Status).ConfigureAwait(false);

            reportForNotification.MarkAsNotificationSent();

            await _settlementReportRepository.AddOrUpdateAsync(reportForNotification)
                .ConfigureAwait(false);
        }
    }

    private Task<IntegrationEvent> CreateAsync(SettlementReport reportForNotification, ReportStatus status)
    {
        ArgumentNullException.ThrowIfNull(reportForNotification);

        var now = DateTime.UtcNow;

        var integrationEvent = new IntegrationEvent(
            Guid.Parse(reportForNotification.RequestId),
            UserNotificationTriggered.EventName,
            UserNotificationTriggered.CurrentMinorVersion,
            new UserNotificationTriggered
            {
                ReasonIdentifier = status switch
                {
                    ReportStatus.Completed => "SettlementReportReadyForDownload",
                    ReportStatus.Failed => "SettlementReportFailed",
                    _ => throw new InvalidOperationException("Sending notification for settlement report with status other than Completed or Failed is not supported"),
                },
                TargetActorId = reportForNotification.ActorId.ToString(),
                TargetUserId = reportForNotification.UserId.ToString(),
                RelatedId = reportForNotification.RequestId,
                OccurredAt = now.ToTimestamp(),
                ExpiresAt = now.AddDays(6).ToTimestamp(),
            });

        return Task.FromResult(integrationEvent);
    }
}
