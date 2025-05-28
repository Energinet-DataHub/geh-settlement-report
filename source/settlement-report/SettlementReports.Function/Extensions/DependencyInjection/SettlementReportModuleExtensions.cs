using Azure.Identity;
using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Application.SettlementReports.Handlers;
using Energinet.DataHub.Reports.Application.SettlementReports.Services;
using Energinet.DataHub.Reports.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Reports.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Reports.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Reports.Infrastructure.Helpers;
using Energinet.DataHub.Reports.Infrastructure.Notifications;
using Energinet.DataHub.Reports.Infrastructure.Persistence;
using Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReportRequest;
using Energinet.DataHub.Reports.Infrastructure.Services;
using Energinet.DataHub.Reports.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GridAreaOwnershipAssigned = Energinet.DataHub.Reports.Infrastructure.Contracts.GridAreaOwnershipAssigned;

namespace SettlementReports.Function.Extensions.DependencyInjection;

public static class SettlementReportModuleExtensions
{
    public static IServiceCollection AddSettlementReportFunctionModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<IntegrationEventsOptions>()
            .BindConfiguration(IntegrationEventsOptions.SectionName)
            .ValidateDataAnnotations();

        services
            .AddOptions<ServiceBusNamespaceOptions>()
            .BindConfiguration(ServiceBusNamespaceOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddScoped<IGridAreaOwnershipAssignedEventStore, GridAreaOwnerRepository>();
        services.AddSubscriber<IntegrationEventSubscriptionHandler>(
        [
            GridAreaOwnershipAssigned.Descriptor,
        ]);

        // settlement report services
        services.AddScoped<IRequestSettlementReportJobHandler, RequestSettlementReportJobHandler>();
        services.AddScoped<ISettlementReportDatabaseContext, SettlementReportDatabaseContext>();
        services.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        services.AddScoped<IGetSettlementReportsHandler, GetSettlementReportsHandler>();
        services.AddScoped<IRemoveExpiredSettlementReports, RemoveExpiredSettlementReports>();
        services.AddScoped<ISettlementReportDatabricksJobsHelper, SettlementReportDatabricksJobsHelper>();
        services.AddScoped<ISettlementReportPersistenceService, SettlementReportPersistenceService>();
        services.AddScoped<IListSettlementReportJobsHandler, ListSettlementReportJobsHandler>();
        services.AddScoped<ISettlementReportFileService, SettlementReportFileService>();
        services.AddSettlementReportBlobStorage();

        // Database Health check
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

        services.TryAddHealthChecks(
            registrationKey: HealthCheckNames.SettlementReportDatabase,
            (key, builder) =>
            {
                builder.AddDbContextCheck<SettlementReportDatabaseContext>(name: key);
            });

        AddHealthChecks(services);

        services.AddServiceBusClientForApplication(configuration, _ => new DefaultAzureCredential());
        services.AddIntegrationEventsPublisher<IntegrationEventProvider>(configuration);

        return services;
    }

    private static void AddHealthChecks(IServiceCollection services)
    {
        services
            .AddHealthChecks();
    }
}
