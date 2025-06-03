using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.MeasurementsReport;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.States;

public class MeasurementsReportScenarioState
{
    public MeasurementsReportRequestDto? MeasurementsReportRequestDto { get; set; }

    public JobRunId? JobRunId { get; set; }
}
