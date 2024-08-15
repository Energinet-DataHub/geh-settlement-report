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
using Energinet.DataHub.SettlementReport.Calculations.Application;
using Energinet.DataHub.SettlementReport.Calculations.Infrastructure.Calculations;
using Energinet.DataHub.SettlementReport.Calculations.Infrastructure.CalculationState;
using Energinet.DataHub.SettlementReport.Calculations.Infrastructure.Persistence;
using Energinet.DataHub.SettlementReport.Calculations.Infrastructure.Persistence.GridArea;
using Energinet.DataHub.SettlementReport.Calculations.Interfaces;
using Energinet.DataHub.SettlementReport.Calculations.Interfaces.GridArea;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.SettlementReport.Calculations.Infrastructure.Extensions.DependencyInjection;

/// <summary>
/// Registration of services required for the Calculations module.
/// </summary>
public static class CalculationsExtensions
{
    /// <summary>
    /// Dependencies solely needed for the orchestration of calculations.
    /// </summary>
    public static IServiceCollection AddCalculationsOrchestrationModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDatabricksJobsForApplication(configuration);

        services.AddSingleton(new CalculationStateMapper());

        services.AddScoped<IDatabricksCalculatorJobSelector, DatabricksCalculatorJobSelector>();

        return services;
    }

    /// <summary>
    /// Dependencies needed for retrieving and updating information about calculations.
    /// </summary>
    public static IServiceCollection AddCalculationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddScoped<IGridAreaOwnerRepository, GridAreaOwnerRepository>();

        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddDbContext<DatabaseContext>(
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
            registrationKey: HealthCheckNames.WholesaleDatabase,
            (key, builder) =>
            {
                builder.AddDbContextCheck<DatabaseContext>(name: key);
            });

        services.AddScoped<IGridAreaOwnershipClient, GridAreaOwnershipClient>();

        return services;
    }
}
