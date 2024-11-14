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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.SettlementReport.Infrastructure.Extensions.Options;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence;
using Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.IntegrationTests.DurableTask;
using Energinet.DataHub.SettlementReport.Test.Core.Fixture.Database;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit.Abstractions;

namespace Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.IntegrationTests.Fixtures;

public class OrchestrationSettlementReportsAppFixture : IAsyncLifetime
{
    /// <summary>
    /// Durable Functions Task Hub Name
    /// See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
    /// </summary>
    private const string TaskHubName = "WholesaleTest02";

    public OrchestrationSettlementReportsAppFixture()
    {
        TestLogger = new TestDiagnosticsLogger();
        IntegrationTestConfiguration = new IntegrationTestConfiguration();

        AzuriteManager = new AzuriteManager(useOAuth: true);
        DatabaseManager = new WholesaleDatabaseManager<SettlementReportDatabaseContext>();

        DurableTaskManager = new DurableTaskManager(
            "AzureWebJobsStorage",
            AzuriteManager.FullConnectionString);

        HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();

        MockServer = WireMockServer.Start(port: 2048);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public ITestDiagnosticsLogger TestLogger { get; }

    public WireMockServer MockServer { get; }

    [NotNull]
    public FunctionAppHostManager? AppHostManager { get; private set; }

    [NotNull]
    public IDurableClient? DurableClient { get; private set; }

    public WholesaleDatabaseManager<SettlementReportDatabaseContext> DatabaseManager { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    private AzuriteManager AzuriteManager { get; }

    private DurableTaskManager DurableTaskManager { get; }

    private FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

    public async Task InitializeAsync()
    {
        // Storage emulator
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AzuriteManager.StartAzurite();

        // Database
        await DatabaseManager.CreateDatabaseAsync();

        // Prepare host settings
        var port = 8100;
        var appHostSettings = CreateAppHostSettings(ref port);

        await EnsureSettlementReportStorageContainerExistsAsync();
        EnsureRevisionLogRespondsWithSuccess();

        // Create and start host
        AppHostManager = new FunctionAppHostManager(appHostSettings, TestLogger);
        StartHost(AppHostManager);

        // Create durable client when TaskHub has been created
        DurableClient = DurableTaskManager.CreateClient(taskHubName: TaskHubName);
    }

    public async Task DisposeAsync()
    {
        AppHostManager?.Dispose();
        MockServer.Dispose();
        DurableTaskManager.Dispose();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AzuriteManager.Dispose();

        await DatabaseManager.DeleteDatabaseAsync();
    }

    public void EnsureAppHostUsesMockedDatabricksJobs()
    {
        AppHostManager?.RestartHostIfChanges(new Dictionary<string, string>
        {
            {
                nameof(DatabricksJobsOptions.WorkspaceUrl), MockServer.Url!
            },
        });
    }

    public void EnsureRevisionLogRespondsWithSuccess()
    {
        var requestRevision = Request
            .Create()
            .WithPath("/revision-log")
            .UsingPost();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK);

        MockServer
            .Given(requestRevision)
            .RespondWith(response);
    }

    public BlobContainerClient CreateBlobContainerClient()
    {
        return new BlobContainerClient(AzuriteManager.FullConnectionString, "settlement-report-container");
    }

    /// <summary>
    /// Use this method to attach <paramref name="testOutputHelper"/> to the host logging pipeline.
    /// While attached, any entries written to host log pipeline will also be logged to xUnit test output.
    /// It is important that it is only attached while a test i active. Hence, it should be attached in
    /// the test class constructor; and detached in the test class Dispose method (using 'null').
    /// </summary>
    /// <param name="testOutputHelper">If a xUnit test is active, this should be the instance of xUnit's <see cref="ITestOutputHelper"/>;
    /// otherwise it should be 'null'.</param>
    public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        TestLogger.TestOutputHelper = testOutputHelper;
    }

    private FunctionAppHostSettings CreateAppHostSettings(ref int port)
    {
        const string project = "Orchestration.SettlementReports";

        var buildConfiguration = GetBuildConfiguration();

        var appHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
        appHostSettings.FunctionApplicationPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"..\\..\\..\\..\\{project}\\bin\\{buildConfiguration}\\net8.0"
            : $"../../../../{project}/bin/{buildConfiguration}/net8.0";
        appHostSettings.Port = ++port;

        // It seems the host + worker is not ready if we use the default startup log message, so we override it here
        appHostSettings.HostStartedEvent = "Host lock lease acquired";

        appHostSettings.ProcessEnvironmentVariables.Add(
            "FUNCTIONS_WORKER_RUNTIME",
            "dotnet-isolated");
        appHostSettings.ProcessEnvironmentVariables.Add(
            "AzureWebJobsStorage",
            AzuriteManager.FullConnectionString);
        appHostSettings.ProcessEnvironmentVariables.Add(
            "DURABLETASK_STORAGE_CONNECTION_STRING",
            AzuriteManager.FullConnectionString);
        appHostSettings.ProcessEnvironmentVariables.Add(
            "APPLICATIONINSIGHTS_CONNECTION_STRING",
            IntegrationTestConfiguration.ApplicationInsightsConnectionString);

        // Durable Functions Task Hub Name
        appHostSettings.ProcessEnvironmentVariables.Add(
            "OrchestrationsTaskHubName",
            TaskHubName);

        // Database
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{ConnectionStringsOptions.ConnectionStrings}__{nameof(ConnectionStringsOptions.DB_CONNECTION_STRING)}",
            DatabaseManager.ConnectionString);

        // Databricks
        // => Notice we reconfigure this setting in "EnsureAppHostUsesActualDatabricksJobs" and "EnsureAppHostUsesMockedDatabricksJobs"
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DatabricksJobsOptions.WorkspaceUrl),
            MockServer.Url!);
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DatabricksJobsOptions.WorkspaceToken),
            IntegrationTestConfiguration.DatabricksSettings.WorkspaceAccessToken);
        // => Only SQL Statement needs Warehouse
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DatabricksSqlStatementOptions.WarehouseId),
            IntegrationTestConfiguration.DatabricksSettings.WarehouseId);

        // DataLake
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DataLakeOptions.STORAGE_ACCOUNT_URI),
            AzuriteManager.BlobStorageServiceUri.ToString());
        appHostSettings.ProcessEnvironmentVariables.Add(
            nameof(DataLakeOptions.STORAGE_CONTAINER_NAME),
            "wholesale");

        // Settlement Report blob storage configuration
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{SettlementReportStorageOptions.SectionName}__{nameof(SettlementReportStorageOptions.StorageContainerName)}",
            "settlement-report-container");
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{SettlementReportStorageOptions.SectionName}__{nameof(SettlementReportStorageOptions.StorageAccountUri)}",
            AzuriteManager.BlobStorageServiceUri + "/");

        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{SettlementReportStorageOptions.SectionName}__{nameof(SettlementReportStorageOptions.StorageContainerForJobsName)}",
            "settlement-report-container-jobs");
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"{SettlementReportStorageOptions.SectionName}__{nameof(SettlementReportStorageOptions.StorageAccountForJobsUri)}",
            AzuriteManager.BlobStorageServiceUri + "/");

        // Revision log
        appHostSettings.ProcessEnvironmentVariables.Add(
            $"RevisionLogOptions:ApiAddress",
            MockServer.Url! + "/revision-log");

        // Integration events
        appHostSettings.ProcessEnvironmentVariables.Add($"{IntegrationEventsOptions.SectionName}__{nameof(IntegrationEventsOptions.SubscriptionName)}", "fake_value");
        appHostSettings.ProcessEnvironmentVariables.Add($"{IntegrationEventsOptions.SectionName}__{nameof(IntegrationEventsOptions.TopicName)}", "fake_value");
        appHostSettings.ProcessEnvironmentVariables.Add($"{ServiceBusNamespaceOptions.SectionName}__{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}", "fake_value");

        return appHostSettings;
    }

    private static void StartHost(FunctionAppHostManager hostManager)
    {
        try
        {
            hostManager.StartHost();
        }
        catch (Exception)
        {
            // Function App Host failed during startup.
            // Exception has already been logged by host manager.
            var unused = hostManager.GetHostLogSnapshot();

            if (Debugger.IsAttached)
                Debugger.Break();

            // Rethrow
            throw;
        }
    }

    private async Task EnsureSettlementReportStorageContainerExistsAsync()
    {
        // Uses BlobStorageConnectionString instead of Uri and DefaultAzureCredential for faster test execution
        // (new DefaultAzureCredential() takes >30 seconds to check credentials)
        var blobClient = new BlobServiceClient(AzuriteManager.BlobStorageConnectionString);
        var blobContainerClient = blobClient.GetBlobContainerClient("settlement-report-container");
        var containerExists = await blobContainerClient.ExistsAsync();
        if (!containerExists)
            await blobContainerClient.CreateAsync();
    }

    private static string GetBuildConfiguration()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }
}
