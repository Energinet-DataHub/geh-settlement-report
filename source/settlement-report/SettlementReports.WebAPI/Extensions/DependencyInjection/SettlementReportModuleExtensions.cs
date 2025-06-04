using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Reports.Application.MeasurementsReport;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Handlers;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Services;
using Energinet.DataHub.Reports.Application.Services;
using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Application.SettlementReports.Handlers;
using Energinet.DataHub.Reports.Application.SettlementReports.Services;
using Energinet.DataHub.Reports.Common.Infrastructure.Extensions.Options;
using Energinet.DataHub.Reports.Common.Infrastructure.HealthChecks;
using Energinet.DataHub.Reports.Infrastructure.Extensions.DependencyInjection;
using Energinet.DataHub.Reports.Infrastructure.Helpers;
using Energinet.DataHub.Reports.Infrastructure.Persistence;
using Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;
using Energinet.DataHub.Reports.Infrastructure.Persistence.SettlementReport;
using Energinet.DataHub.Reports.Infrastructure.Services;
using Energinet.DataHub.Reports.Interfaces;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Reports.WebAPI.Extensions.DependencyInjection;

public static class SettlementReportModuleExtensions
{
    public static IServiceCollection AddSettlementReportApiModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // general services
        services.AddScoped<IGridAreaOwnerRepository, GridAreaOwnerRepository>();

        // settlement report services
        services.AddScoped<IRequestSettlementReportJobHandler, RequestSettlementReportJobHandler>();
        services.AddScoped<IReportsDatabaseContext, ReportsDatabaseContext>();
        services.AddScoped<ISettlementReportRepository, SettlementReportRepository>();
        services.AddScoped<IGetSettlementReportsHandler, GetSettlementReportsHandler>();
        services.AddScoped<IRemoveExpiredSettlementReports, RemoveExpiredSettlementReports>();
        services.AddScoped<ISettlementReportDatabricksJobsHelper, SettlementReportDatabricksJobsHelper>();
        services.AddScoped<ISettlementReportPersistenceService, SettlementReportPersistenceService>();
        services.AddScoped<IListSettlementReportJobsHandler, ListSettlementReportJobsHandler>();
        services.AddScoped<ISettlementReportFileService, SettlementReportFileService>();
        services.AddScoped<ICancelSettlementReportJobHandler, CancelSettlementReportJobHandler>();
        services.AddSettlementReportBlobStorage();

        // measurements reports services
        services.AddScoped<IRequestMeasurementsReportHandler, RequestMeasurementsReportHandler>();
        services.AddScoped<IMeasurementsReportDatabricksJobsHelper, MeasurementsReportDatabricksJobsHelper>();
        services.AddScoped<IMeasurementsReportRepository, MeasurementsReportRepository>();
        services.AddScoped<IMeasurementsReportFileService, MeasurementsReportFileService>();
        services.AddScoped<IListMeasurementsReportService, ListMeasurementsReportService>();
        services.AddScoped<IMeasurementsReportService, MeasurementsReportService>();

        // Database Health check
        services.AddDbContext<ReportsDatabaseContext>(options => options.UseSqlServer(
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
                builder.AddDbContextCheck<ReportsDatabaseContext>(name: key);
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
