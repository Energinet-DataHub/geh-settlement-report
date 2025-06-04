using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
using Energinet.DataHub.Reports.Client;
using Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.States;
using Energinet.DataHub.Reports.SubsystemTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.Fixtures;

public class MeasurementsReportScenarioFixture : IAsyncLifetime
{
    public MeasurementsReportScenarioFixture()
    {
        Logger = new TestDiagnosticsLogger();

        Configuration = new ReportsSubsystemTestConfiguration();
        ScenarioState = new MeasurementsReportScenarioState();
    }

    /// <summary>
    /// The actual client is not created until <see cref="InitializeAsync"/> has been called by the base class.
    /// </summary>
    public IMeasurementsReportClient MeasurementsReportClient { get; private set; } = null!;

    public MeasurementsReportScenarioState ScenarioState { get; }

    public ReportsSubsystemTestConfiguration Configuration { get; }

    private TestDiagnosticsLogger Logger { get; }

    public async Task InitializeAsync()
    {
        MeasurementsReportClient = await MeasurementsReportClientFactory.CreateAsync(Configuration);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public void SetTestOutputHelper(ITestOutputHelper? testOutputHelper)
    {
        Logger.TestOutputHelper = testOutputHelper;
    }

    public async Task<(bool IsCompletedOrFailed, RequestedMeasurementsReportDto? ReportRequest)> WaitForReportGenerationCompletedOrFailedAsync(
        JobRunId jobRunId,
        TimeSpan waitTimeLimit)
    {
        var delay = TimeSpan.FromSeconds(30);

        RequestedMeasurementsReportDto? reportRequest = null;

        var isCompletedOrFailed = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                reportRequest = await GetReportRequestByJobRunIdAsync(jobRunId);
                return reportRequest?.Status is ReportStatus.Completed or ReportStatus.Failed;
            },
            waitTimeLimit,
            delay);

        return (isCompletedOrFailed, reportRequest);
    }

    public async Task<RequestedMeasurementsReportDto?> GetReportRequestByJobRunIdAsync(JobRunId jobRunId)
    {
        var reportRequests = await MeasurementsReportClient.GetAsync(CancellationToken.None);
        return reportRequests.FirstOrDefault(x => x.JobRunId is not null && x.JobRunId.Id == jobRunId.Id);
    }
}
