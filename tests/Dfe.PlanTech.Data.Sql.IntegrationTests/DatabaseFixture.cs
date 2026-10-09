using DbUp;
using Dfe.PlanTech.Core.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

public class DatabaseFixture : IAsyncLifetime
{
    // TEST CONTAINER ONLY: This password is only for the ephemeral test SQL Server
    // spun up via test containers for integration tests. It is NOT used in production,
    // development, or any persistent environment.
    // This password is safe to commit and is not a security risk.
    private const string TestContainerPassword = "yourStrong(!)Password";

    public MsSqlContainer DbContainer { get; private set; } = null!;
    public string ConnectionString => DbContainer.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        DbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(TestContainerPassword)
            .Build();
        await DbContainer.StartAsync();

        var filterOutput = new List<string>();

        // Run DbUp migrations
        EnsureDatabase.For.SqlDatabase(ConnectionString);
        var upgrader = DeployChanges
            .To.SqlDatabase(ConnectionString)
            .WithScriptsEmbeddedInAssembly(
                typeof(DatabaseUpgrader.Options).Assembly,
                s =>
                {
                    filterOutput.Add(s);
                    return s.StartsWith(
                        "Dfe.PlanTech.DatabaseUpgrader.Scripts",
                        StringComparison.Ordinal
                    );
                }
            )
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
    /// <summary>
    /// Creates a context for a test. The user action id provider is opt-in and defaults to null,
    /// matching the non-web hosts (such as SeedTestData) that construct the context without one.
    /// Supply a provider when a test needs to assert that userActionId is stamped on save.
    /// </summary>
    public PlanTechDbContext CreateDbContext(IUserActionIdProvider? userActionIdProvider = null)
    {
        var options = new DbContextOptionsBuilder<PlanTechDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new PlanTechDbContext(options, userActionIdProvider);
    }

    public async ValueTask DisposeAsync()
    {
        await DbContainer.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
