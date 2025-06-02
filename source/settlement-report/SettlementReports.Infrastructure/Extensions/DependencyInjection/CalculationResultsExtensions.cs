using Azure.Identity;
using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Application.SettlementReports.Handlers;
using Energinet.DataHub.Reports.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Reports.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Reports.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Reports.Infrastructure.Extensions.Options;
using Energinet.DataHub.Reports.Infrastructure.Helpers;
using Energinet.DataHub.Reports.Infrastructure.Notifications;
using Energinet.DataHub.Reports.Infrastructure.Persistence;
using Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReportRequest;
using Energinet.DataHub.Reports.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Reports.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the CalculationResults module.
/// </summary>
public static class CalculationResultsExtensions
{
    public static IServiceCollection AddSettlementReportsV2Module(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<IntegrationEventsOptions>().BindConfiguration(IntegrationEventsOptions.SectionName).ValidateDataAnnotations();
        services.AddOptions<ServiceBusNamespaceOptions>().BindConfiguration(ServiceBusNamespaceOptions.SectionName).ValidateDataAnnotations();

        services.AddDatabricksSqlStatementForApplication(configuration);

        // Settlement Reports
        services.AddScoped<ISettlementReportRequestHandler, SettlementReportRequestHandler>();
        services.AddScoped<ISettlementReportFileRequestHandler, SettlementReportFileRequestHandler>();
        services.AddScoped<ISettlementReportFromFilesHandler, SettlementReportFromFilesHandler>();
        services.AddScoped<ISettlementReportFinalizeHandler, SettlementReportFinalizeHandler>();
        services.AddScoped<ISettlementReportPersistenceService, SettlementReportPersistenceService>();

        services.AddScoped<IGetSettlementReportsHandler, GetSettlementReportsHandler>();
        services.AddScoped<ISettlementReportDownloadHandler, SettlementReportDownloadHandler>();
        services.AddScoped<IUpdateFailedSettlementReportsHandler, UpdateFailedSettlementReportsHandler>();

        services.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        services.AddScoped<IRemoveExpiredSettlementReports, RemoveExpiredSettlementReports>();
        services.AddScoped<ISettlementReportFileGeneratorFactory, SettlementReportFileGeneratorFactory>();
        services.AddScoped<IListSettlementReportJobsHandler, ListSettlementReportJobsHandler>();
        services.AddScoped<ISettlementReportDatabricksJobsHelper, SettlementReportDatabricksJobsHelper>();
        services.AddSettlementReportBlobStorage();
        services.AddScoped<ISettlementReportDatabaseContext, SettlementReportDatabaseContext>();
        services.AddDbContext<SettlementReportDatabaseContext>(
            options => options.UseSqlServer(
                configuration
                    .GetSection(ConnectionStringsOptions.ConnectionStrings)
                    .Get<ConnectionStringsOptions>()!.DB_CONNECTION_STRING,
                o =>
                {
                    o.UseNodaTime();
                    o.EnableRetryOnFailure();
                }));

        // Database Health check
        services.TryAddHealthChecks(
            registrationKey: HealthCheckNames.SettlementReportDatabase,
            (key, builder) =>
            {
                builder.AddDbContextCheck<SettlementReportDatabaseContext>(name: key);
                builder.AddAzureServiceBusSubscription(
                    provider => provider.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value
                        .FullyQualifiedNamespace,
                    provider => provider.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.TopicName,
                    provider => provider.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value
                        .SubscriptionName,
                    _ => new DefaultAzureCredential());
            });

        // Used by sql statements (queries)
        services.AddOptions<DeltaTableOptions>().Bind(configuration);

        services.AddServiceBusClientForApplication(configuration, _ => new DefaultAzureCredential());
        services.AddIntegrationEventsPublisher<IntegrationEventProvider>(configuration);

        return services;
    }
}
