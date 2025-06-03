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

using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.States;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;

public class SettlementReportScenarioFixture : IAsyncLifetime
{
    public SettlementReportScenarioFixture()
    {
        Logger = new TestDiagnosticsLogger();

        Configuration = new SettlementReportSubsystemTestConfiguration();
        SettlementReportScenarioState = new SettlementReportScenarioState();
    }

    /// <summary>
    /// The actual client is not created until <see cref="InitializeAsync"/> has been called by the base class.
    /// </summary>
    public ISettlementReportClient SettlementReportClient { get; private set; } = null!;

    public SettlementReportScenarioState SettlementReportScenarioState { get; }

    public SettlementReportSubsystemTestConfiguration Configuration { get; }

    private TestDiagnosticsLogger Logger { get; }

    public async Task InitializeAsync()
    {
        SettlementReportClient = await SettlementReportClientFactory.CreateSettlementReportClientAsync(Configuration);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Wait for the report generation to complete or fail.
    /// </summary>
    /// <returns>IsCompletedOrFailed: True if the report generation completed or failed; otherwise false.</returns>
    public async Task<(bool IsCompletedOrFailed, RequestedSettlementReportDto? ReportRequest)> WaitForReportGenerationCompletedOrFailedAsync(
        JobRunId jobRunId,
        TimeSpan waitTimeLimit)
    {
        var delay = TimeSpan.FromSeconds(30);

        RequestedSettlementReportDto? reportRequest = null;

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

    public void SetTestOutputHelper(ITestOutputHelper? testOutputHelper)
    {
        Logger.TestOutputHelper = testOutputHelper;
    }

    public async Task<RequestedSettlementReportDto?> GetReportRequestByJobRunIdAsync(JobRunId jobRunId)
    {
        var reportRequests = await SettlementReportClient.GetAsync(CancellationToken.None);
        return reportRequests.FirstOrDefault(x => x.JobId is not null && x.JobId.Id == jobRunId.Id);
    }
}
