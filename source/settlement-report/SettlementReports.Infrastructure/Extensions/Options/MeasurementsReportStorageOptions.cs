using System.ComponentModel.DataAnnotations;

namespace Energinet.DataHub.Reports.Infrastructure.Extensions.Options;

public class MeasurementsReportStorageOptions
{
    public const string SectionName = "MeasurementsReportStorage";

    [Required]
    public Uri StorageAccountForJobsUri { get; set; } = null!;

    [Required]
    public string StorageContainerForJobsName { get; set; } = string.Empty;
}
