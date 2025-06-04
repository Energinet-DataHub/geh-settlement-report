using System.Reflection;
using DbUp;
using DbUp.Engine;

namespace Energinet.DataHub.SettlementReport.DatabaseMigration;

public static class Upgrader
{
    public static DatabaseUpgradeResult DatabaseUpgrade(string? connectionString)
    {
        var upgrader =
            DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

        var result = upgrader.PerformUpgrade();
        return result;
    }
}
