using Contentful.Core.Configuration;
using Dfe.PlanTech.Data.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.SeedTestData;

public static class Program
{
    private static ServiceProvider CreateServiceProvider()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddUserSecrets("f2f25d53-aecc-4c9f-9aef-3b2447db4f97");


        var configuration = configurationBuilder.Build();
        var services = new ServiceCollection();

        services.AddDbContext<PlanTechDbContext>(opts =>
        {
            opts.UseSqlServer(configuration.GetConnectionString("Database"));
            opts.EnableSensitiveDataLogging();
        });

        services.AddLogging(opts =>
        {
            opts.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddSingleton(new ContentfulOptions());

        services.AddSingleton<SeedData>();

        return services.BuildServiceProvider();
    }

    public static void Main()
    {
        var provider = CreateServiceProvider();
        var seeder = provider.GetRequiredService<SeedData>();
        seeder.CreateData();
    }
}
