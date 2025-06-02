using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Reports.Common.Infrastructure.Telemetry;
using Energinet.DataHub.Reports.Function.Extensions.DependencyInjection;
using Energinet.DataHub.RevisionLog.Integration.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
