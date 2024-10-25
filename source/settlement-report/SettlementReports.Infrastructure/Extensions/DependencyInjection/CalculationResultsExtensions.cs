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

using Azure.Identity;
using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Options;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence.Databricks;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence.SettlementReportRequest;
using Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the CalculationResults module.
/// </summary>
public static class CalculationResultsExtensions
{
    public static IServiceCollection AddSettlementReportsV2Module(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ServiceBusNamespaceOptions>()
            .BindConfiguration(ServiceBusNamespaceOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddDatabricksSqlStatementForApplication(configuration);

        // Settlement Reports
        services.AddScoped<ISettlementReportRequestHandler, SettlementReportRequestHandler>();
        services.AddScoped<ISettlementReportFileRequestHandler, SettlementReportFileRequestHandler>();
        services.AddScoped<ISettlementReportFromFilesHandler, SettlementReportFromFilesHandler>();
        services.AddScoped<ISettlementReportFinalizeHandler, SettlementReportFinalizeHandler>();
        services.AddScoped<ISettlementReportInitializeHandler, SettlementReportInitializeHandler>();

        services.AddScoped<IGetSettlementReportsHandler, GetSettlementReportsHandler>();
        services.AddScoped<ISettlementReportDownloadHandler, SettlementReportDownloadHandler>();
        services.AddScoped<IUpdateFailedSettlementReportsHandler, UpdateFailedSettlementReportsHandler>();

        services.AddScoped<ISettlementReportDatabricksContext, SettlementReportDatabricksContext>();
        services.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        services.AddScoped<IRemoveExpiredSettlementReports, RemoveExpiredSettlementReports>();
        services.AddScoped<ISettlementReportFileGeneratorFactory, SettlementReportFileGeneratorFactory>();
        services.AddScoped<ISettlementReportWholesaleRepository, SettlementReportWholesaleRepository>();
        services.AddScoped<ISettlementReportEnergyResultRepository, SettlementReportEnergyResultRepository>();
        services.AddScoped<ISettlementReportMeteringPointTimeSeriesResultRepository, SettlementReportMeteringPointTimeSeriesResultRepository>();
        services.AddScoped<ISettlementReportChargeLinkPeriodsRepository, SettlementReportChargeLinkPeriodsRepository>();
        services.AddScoped<ISettlementReportMeteringPointMasterDataRepository, SettlementReportMeteringPointMasterDataRepository>();
        services.AddScoped<ISettlementReportMonthlyAmountRepository, SettlementReportMonthlyAmountRepository>();
        services.AddScoped<ILatestCalculationVersionRepository, LatestCalculationVersionRepository>();
        services.AddScoped<ISettlementReportChargePriceRepository, SettlementReportChargePriceRepository>();
        services.AddScoped<ISettlementReportMonthlyAmountTotalRepository, SettlementReportMonthlyAmountTotalRepository>();
        services.AddSettlementReportBlobStorage();
        services.AddServiceBusClientForApplication(configuration);
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

        return services;
    }
}
