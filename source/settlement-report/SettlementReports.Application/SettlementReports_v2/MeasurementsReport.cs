namespace Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;

public sealed class MeasurementsReport
{
    public MeasurementsReport(string? blobFileName = null)
    {
        BlobFileName = blobFileName;
    }

    public string? BlobFileName { get; private set; }
}
