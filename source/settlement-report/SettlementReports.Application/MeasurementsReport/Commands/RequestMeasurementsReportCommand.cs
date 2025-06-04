using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Commands;

public sealed record RequestMeasurementsReportCommand(
    MeasurementsReportRequestDto RequestDto,
    Guid UserId,
    Guid ActorId,
    string ActorGln);
