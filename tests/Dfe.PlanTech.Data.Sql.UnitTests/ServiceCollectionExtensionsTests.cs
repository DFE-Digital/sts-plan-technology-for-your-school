using System.Reflection;
using Dfe.PlanTech.Core.Providers.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Data.Sql.UnitTests;

/// <summary>
/// PlanTechDbContext takes its IUserActionIdProvider as an optional constructor parameter that
/// defaults to null, and it also has a parameterless constructor. If the provider registration were
/// removed, or constructor selection changed, stamping would stop silently rather than fail: no
/// exception, just nulls appearing in an audit column. These tests pin that wiring, which nothing
/// else observes.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    private class TestUserActionIdProvider : IUserActionIdProvider
    {
        public Guid GetUserActionId() => Guid.NewGuid();
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = "Server=(localdb)\\test;Database=test;",
                    ["Database:MaxRetryCount"] = "3",
                    ["Database:MaxDelayInMilliseconds"] = "1000",
                }
            )
            .Build();

    [Fact]
    public void AddDatabase_WhenAUserActionIdProviderIsRegistered_ThenTheDbContextReceivesIt()
    {
        var services = new ServiceCollection();
        services.AddScoped<IUserActionIdProvider, TestUserActionIdProvider>();
        services.AddDatabase(BuildConfiguration());

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PlanTechDbContext>();

        Assert.NotNull(GetUserActionIdProvider(dbContext));
    }

    [Fact]
    public void AddDatabase_WhenNoUserActionIdProviderIsRegistered_ThenTheDbContextStillResolves()
    {
        var services = new ServiceCollection();
        services.AddDatabase(BuildConfiguration());

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PlanTechDbContext>();

        // Non-web hosts such as SeedTestData register no provider, so resolution must still
        // succeed. That tolerance is precisely why the registered case above needs pinning.
        Assert.Null(GetUserActionIdProvider(dbContext));
    }

    // The field is private and there is no accessor for it. The invariant is otherwise invisible and
    // its failure mode is silent, which is when reflection in a test is the lesser evil.
    private static IUserActionIdProvider? GetUserActionIdProvider(PlanTechDbContext dbContext)
    {
        var field = typeof(PlanTechDbContext).GetField(
            "_userActionIdProvider",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.NotNull(field);

        return (IUserActionIdProvider?)field.GetValue(dbContext);
    }
}
