using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.States;

public class MeasurementsReportScenarioState
{
    public MeasurementsReportRequestDto? MeasurementsReportRequestDto { get; set; }

    public JobRunId? JobRunId { get; set; }
}
