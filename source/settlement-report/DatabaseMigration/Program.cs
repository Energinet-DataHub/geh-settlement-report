namespace Energinet.DataHub.SettlementReport.DatabaseMigration;

internal static class Program
{
    public static int Main(string[] args)
    {
        // If you are migrating to SQL Server Express use connection string "Server=(LocalDb)\\MSSQLLocalDB;..."
        // If you are migrating to SQL Server use connection string "Server=localhost;..."
        var connectionString =
            args.FirstOrDefault()
            ?? "Server=localhost;Database=settlementreport;Trusted_Connection=True;Encrypt=No;";

        Console.WriteLine($"Performing upgrade on {connectionString}");
        var result = Upgrader.DatabaseUpgrade(connectionString);

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
#if DEBUG
            Console.ReadLine();
#endif
            return -1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
        return 0;
    }
}
