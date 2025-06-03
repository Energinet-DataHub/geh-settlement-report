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
using Azure.Storage.Blobs;
using Energinet.DataHub.Reports.Application.Services;
using Energinet.DataHub.Reports.Application.SettlementReports;
using Energinet.DataHub.Reports.Infrastructure.Extensions.Options;
using Energinet.DataHub.Reports.Infrastructure.Services;
using HealthChecks.Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Reports.Infrastructure.Extensions.DependencyInjection;

public static class StorageExtensions
{
    public static IServiceCollection AddSettlementReportBlobStorage(this IServiceCollection services)
    {
        services
            .AddOptions<SettlementReportStorageOptions>()
            .BindConfiguration(SettlementReportStorageOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddScoped<ISettlementReportFileRepository, SettlementReportFileBlobStorage>(serviceProvider =>
        {
            var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>().Value;

            var blobContainerUri = new Uri(blobSettings.StorageAccountUri, blobSettings.StorageContainerName);
            var blobContainerClient = new BlobContainerClient(blobContainerUri, new DefaultAzureCredential());

            return new SettlementReportFileBlobStorage(blobContainerClient);
        });

        services.AddScoped<IReportFileRepository, ReportFileRepository>(serviceProvider =>
        {
            var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>().Value;

            var blobContainerUri = new Uri(blobSettings.StorageAccountForJobsUri, blobSettings.StorageContainerForJobsName);
            var blobContainerClient = new BlobContainerClient(blobContainerUri, new DefaultAzureCredential());

            return new ReportFileRepository(blobContainerClient);
        });

        // Health checks
        services
            .AddHealthChecks()
            .AddAzureBlobStorage(
                serviceProvider =>
                {
                    var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>();
                    return new BlobServiceClient(blobSettings.Value.StorageAccountUri, new DefaultAzureCredential());
                },
                serviceProvider =>
                {
                    var blobSettings = serviceProvider.GetRequiredService<IOptions<SettlementReportStorageOptions>>();
                    return new AzureBlobStorageHealthCheckOptions { ContainerName = blobSettings.Value.StorageContainerName };
                },
                "SettlementReportBlobStorage")
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
