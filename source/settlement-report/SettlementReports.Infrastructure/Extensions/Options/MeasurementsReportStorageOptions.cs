using System.ComponentModel.DataAnnotations;

namespace Energinet.DataHub.Reports.Infrastructure.Extensions.Options;

public class MeasurementsReportStorageOptions
{
    public const string SectionName = "MeasurementsReportStorage";

    [Required]
    public string StorageContainerForJobsName { get; set; } = string.Empty;

    [Required]
    public string DirectoryPath { get; set; } = string.Empty;
}
