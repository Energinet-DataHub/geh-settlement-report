namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

public sealed record SettlementReportPartialFileInfo
{
    public SettlementReportPartialFileInfo(string fileName, bool preventLargeTextFiles)
    {
        FileName = fileName;
        PreventLargeTextFiles = preventLargeTextFiles;
        FileOffset = 0;
        ChunkOffset = 0;
    }

    public string FileName { get; init; }

    /// <summary>
    /// Specifies that the combined file should not grow so large that it cannot be opened.
    /// </summary>
    public bool PreventLargeTextFiles { get; init; }

    public int FileOffset { get; init; }

    public int ChunkOffset { get; init; }
}
