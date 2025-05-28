using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Reports.Application.SettlementReports.Handlers;
using Microsoft.Azure.Functions.Worker;

namespace SettlementReports.Function.Functions;

/// <summary>
/// Scheduled function that updates the status of settlement reports.
/// This is used to notify the system about terminated settlement reports request (jobs).
/// The main purpose is to add a badge notification in the front-end.
/// </summary>
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
