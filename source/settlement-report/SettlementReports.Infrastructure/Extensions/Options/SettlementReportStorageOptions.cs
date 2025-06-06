﻿using System.ComponentModel.DataAnnotations;

namespace Energinet.DataHub.Reports.Infrastructure.Extensions.Options;

public class SettlementReportStorageOptions
{
    public const string SectionName = "SettlementReportStorage";

    [Required]
    public Uri StorageAccountForJobsUri { get; set; } = null!;

    [Required]
    public string StorageContainerForJobsName { get; set; } = string.Empty;

    [Required]
    public string DirectoryPath { get; set; } = string.Empty;
}
