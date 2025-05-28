using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.RevisionLog.Integration.Extensions.DependencyInjection;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Telemetry;
using Microsoft.Extensions.Hosting;
using SettlementReports.Function.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        // Common
        services.AddApplicationInsightsForIsolatedWorker(SubsystemInformation.SubsystemName);
        services.AddHealthChecksForIsolatedWorker();

        // Shared by modules
        services.AddNodaTimeForApplication();
        services.AddDatabricksJobs(context.Configuration);

        // revision log
        services.AddRevisionLogIntegrationModule(context.Configuration);

        // Modules
        services.AddSettlementReportFunctionModule(context.Configuration);
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext.Configuration);
    })
    .Build();

host.Run();
