using DbUp;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

public class DatabaseFixture : IAsyncLifetime
{
    // TEST CONTAINER ONLY: This password is only for the ephemeral test SQL Server spun up via test containers for integration tests.
    // It is NOT used in production, development, or any persistent environment.
    // This password is safe to commit and is not a security risk.
    private const string TestContainerPassword = "yourStrong(!)Password";

    public MsSqlContainer DbContainer { get; private set; } = null!;
    public string ConnectionString => DbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        DbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(TestContainerPassword)
            .Build();
        await DbContainer.StartAsync();

        // Run DbUp migrations
        EnsureDatabase.For.SqlDatabase(ConnectionString);
        var upgrader = DeployChanges.To
            .SqlDatabase(ConnectionString)
            .WithScriptsEmbeddedInAssembly(
                typeof(Dfe.PlanTech.DatabaseUpgrader.Options).Assembly,
                s => s.StartsWith("Dfe.PlanTech.DatabaseUpgrader.Scripts", StringComparison.Ordinal))
            .LogToConsole()
            .Build();
        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            throw new Exception($"DbUp migration failed: {result.Error}");
        }
    }

    /// <summary>
    /// Creates a new DbContext with transaction isolation for tests.
    /// Each test should call this to get a fresh, isolated context.
    /// </summary>
    public PlanTechDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PlanTechDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new PlanTechDbContext(options);
    }

    public async Task DisposeAsync()
    {
        await DbContainer.DisposeAsync();
    }
}
