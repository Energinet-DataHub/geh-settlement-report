namespace Energinet.DataHub.Reports.Common.Infrastructure.Extensions.Options;

public class ConnectionStringsOptions
{
    // This is the section name. It must match the section name in setting storage.
    public const string ConnectionStrings = "CONNECTIONSTRINGS";

    public string DB_CONNECTION_STRING { get; set; } = string.Empty;
}
