using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.States;
using Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;
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
    public ISettlementReportClient ReportsClient { get; private set; } = null!;

    public MeasurementsReportScenarioState ScenarioState { get; }

    public ReportsSubsystemTestConfiguration Configuration { get; }

    private TestDiagnosticsLogger Logger { get; }

    public async Task InitializeAsync()
    {
        ReportsClient = await SettlementReportClientFactory.CreateSettlementReportClientAsync(Configuration);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public void SetTestOutputHelper(ITestOutputHelper? testOutputHelper)
    {
        Logger.TestOutputHelper = testOutputHelper;
    }
}
