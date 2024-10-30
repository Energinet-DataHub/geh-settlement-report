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
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Google.Protobuf.WellKnownTypes;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Notifications;

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

    private Task<IntegrationEvent> CreateAsync(Application.SettlementReports_v2.SettlementReport reportForNotification, SettlementReportStatus status)
    {
        ArgumentNullException.ThrowIfNull(reportForNotification);

        var now = DateTime.UtcNow;

        var integrationEvent = new IntegrationEvent(
            Guid.Parse(reportForNotification.RequestId),
            Contracts.UserNotificationTriggered.EventName,
            Contracts.UserNotificationTriggered.CurrentMinorVersion,
            new Contracts.UserNotificationTriggered
            {
                ReasonIdentifier = status switch
                {
                    SettlementReportStatus.Completed => "SettlementReportReadyForDownload",
                    SettlementReportStatus.Failed => "SettlementReportFailed",
                    _ => throw new InvalidOperationException("Sending notification for settlement report with status other than Completed or Failed is not supported"),
                },
                TargetActorId = reportForNotification.ActorId.ToString(),
                TargetUserId = reportForNotification.UserId.ToString(),
                RelatedId = reportForNotification.Id.ToString(),
                OccurredAt = now.ToTimestamp(),
                ExpiresAt = now.AddDays(6).ToTimestamp(),
            });

        return Task.FromResult(integrationEvent);
    }
}
