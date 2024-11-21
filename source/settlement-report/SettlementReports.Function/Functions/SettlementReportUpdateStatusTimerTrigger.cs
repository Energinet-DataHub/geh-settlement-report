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

using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.SettlementReport.Application.Handlers;
using Microsoft.Azure.Functions.Worker;

namespace SettlementReports.Function.Functions;

internal sealed class SettlementReportUpdateStatusTimerTrigger
{
    private readonly IListSettlementReportJobsHandler _listSettlementReportJobsHandler;
    private readonly IPublisher _publisher;

    public SettlementReportUpdateStatusTimerTrigger(
        IListSettlementReportJobsHandler listSettlementReportJobsHandler,
        IPublisher publisher)
    {
        _listSettlementReportJobsHandler = listSettlementReportJobsHandler;
        _publisher = publisher;
    }

    [Function(nameof(UpdateStatusForSettlementReports))]
    public async Task UpdateStatusForSettlementReports(
        [TimerTrigger("0 */1 * * * *")] TimerInfo timer,
        FunctionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(executionContext);

        // We are not interested in the result of the handler, as the handler will update the status of the settlement reports
        // It will also handle sending Notifications to the expected recipients
        await _listSettlementReportJobsHandler.HandleAsync().ConfigureAwait(false);
        await _publisher.PublishAsync(executionContext.CancellationToken).ConfigureAwait(false);
    }
}
