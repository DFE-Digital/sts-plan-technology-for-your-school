using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Dfe.PlanTech.Web.SeedTestData;

public static class Program
{
    private static ServiceProvider CreateServiceProvider(IConfiguration? configuration = null)
    {
        configuration ??= CreateConfiguration();

        var services = new ServiceCollection();

        services.AddDbContext<CmsDbContext>(opts =>
        {
            opts.UseSqlServer(configuration.GetConnectionString("Database"));
            opts.EnableSensitiveDataLogging();
        });

        services.AddLogging(opts =>
        {
            opts.AddConsole();
            opts.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddSingleton(new ContentfulOptions(true));

        services.AddSingleton<SeedData>();

        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddUserSecrets<ForLoadingUserSecrets>();

        return configurationBuilder.Build();
    }

    public static void Main()
    {
        var provider = CreateServiceProvider();
        var seeder = provider.GetRequiredService<SeedData>();
        seeder.CreateData();
    }
}

public class ForLoadingUserSecrets
{

}
