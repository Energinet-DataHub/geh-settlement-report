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

using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.SettlementReport.Application.Handlers;
using Energinet.DataHub.SettlementReport.Application.Services;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.SettlementReport.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.SettlementReport.Infrastructure.Helpers;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence.SettlementReportRequest;
using Energinet.DataHub.SettlementReport.Infrastructure.Services;
using Energinet.DataHub.SettlementReport.Infrastructure.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Microsoft.EntityFrameworkCore;

namespace SettlementReports.WebAPI.Extensions.DependencyInjection;

public static class SettlementReportModuleExtensions
{
    public static IServiceCollection AddSettlementReportApiModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // general services
        services.AddScoped<IRequestSettlementReportJobHandler, RequestSettlementReportHandler>();
        services.AddScoped<ISettlementReportDatabaseContext, SettlementReportDatabaseContext>();
        services.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        services.AddScoped<IGetSettlementReportsHandler, GetSettlementReportsHandler>();
        services.AddScoped<IRemoveExpiredSettlementReports, RemoveExpiredSettlementReports>();
        services.AddScoped<IDatabricksJobsHelper, DatabricksJobsHelper>();
        services.AddScoped<ISettlementReportInitializeHandler, SettlementReportInitializeHandler>();
        services.AddScoped<IListSettlementReportJobsHandler, ListSettlementReportJobsHandler>();
        services.AddScoped<IRequestSettlementReportJobHandler, RequestSettlementReportHandler>();
        services.AddScoped<ISettlementReportJobsDownloadHandler, SettlementReportJobsDownloadHandler>();
        services.AddScoped<ICancelSettlementReportJobHandler, CancelSettlementReportJobHandler>();
        services.AddScoped<IGridAreaOwnerRepository, GridAreaOwnerRepository>();
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

        return services;
    }

    private static void AddHealthChecks(IServiceCollection services)
    {
        services
            .AddHealthChecks();
    }
}
