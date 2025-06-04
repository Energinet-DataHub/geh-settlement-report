using Azure.Identity;
using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.MeasurementsReport;
using Energinet.DataHub.Reports.Infrastructure.Extensions.Options;
using Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;
using Energinet.DataHub.Reports.Infrastructure.Services;
using HealthChecks.Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ISettlementReportFileRepository = Energinet.DataHub.Reports.Application.Services.ISettlementReportFileRepository;

namespace Energinet.DataHub.Reports.Infrastructure.Extensions.DependencyInjection;

public static class StorageExtensions
{
    public static IServiceCollection AddSettlementReportBlobStorage(this IServiceCollection services)
    {
        services
            .AddOptions<SettlementReportStorageOptions>()
            .BindConfiguration(SettlementReportStorageOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddScoped<ISettlementReportFileRepository, SettlementReportFileRepository>(serviceProvider =>
        {
            var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>().Value;
            var blobContainerUri = new Uri(blobSettings.StorageAccountForJobsUri, blobSettings.StorageContainerForJobsName);
            var blobContainerClient = new BlobContainerClient(blobContainerUri, new DefaultAzureCredential());

            return new SettlementReportFileRepository(blobContainerClient);
        });

        services.AddScoped<IMeasurementsReportFileRepository, MeasurementsReportFileRepository>(serviceProvider =>
        {
            var blobSettings = serviceProvider.GetRequiredService<IOptions<MeasurementsReportStorageOptions>>().Value;
            var blobContainerUri = new Uri(blobSettings.StorageAccountForJobsUri, blobSettings.StorageContainerForJobsName);
            var blobContainerClient = new BlobContainerClient(blobContainerUri, new DefaultAzureCredential());

            return new MeasurementsReportFileRepository(blobContainerClient);
        });

        // Health checks
        services
            .AddHealthChecks()
            .AddAzureBlobStorage(
                serviceProvider =>
                {
                    var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>();
                    return new BlobServiceClient(blobSettings.Value.StorageAccountForJobsUri, new DefaultAzureCredential());
                },
                serviceProvider =>
                {
                    var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>();
                    return new AzureBlobStorageHealthCheckOptions { ContainerName = blobSettings.Value.StorageContainerForJobsName };
                },
                "SettlementReportBlobStorageJobs");

        return services;
    }
}
