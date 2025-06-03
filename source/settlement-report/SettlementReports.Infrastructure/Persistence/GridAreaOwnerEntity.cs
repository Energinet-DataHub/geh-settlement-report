namespace Energinet.DataHub.Reports.Infrastructure.Persistence;

public class GridAreaOwnerEntity
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string ActorNumber { get; set; } = null!;

    public DateTimeOffset ValidFrom { get; set; }

    public int SequenceNumber { get; set; }
}
