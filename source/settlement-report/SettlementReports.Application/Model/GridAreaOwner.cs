using NodaTime;

namespace Energinet.DataHub.Reports.Application.Model;

public sealed record GridAreaOwner(GridAreaCode Code, ActorNumber ActorNumber, Instant ValidFrom);
