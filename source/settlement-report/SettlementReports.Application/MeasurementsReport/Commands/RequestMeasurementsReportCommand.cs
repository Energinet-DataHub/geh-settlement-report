using Energinet.DataHub.Reports.Interfaces.Models.MeasurementsReport;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Commands;

public sealed record RequestMeasurementsReportCommand(
    MeasurementsReportRequestDto RequestDto,
    Guid UserId,
    Guid ActorId,
    string ActorGln);
