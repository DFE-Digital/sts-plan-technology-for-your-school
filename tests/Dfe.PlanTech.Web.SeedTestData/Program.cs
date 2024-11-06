using System.Reflection;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
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
        configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly());

        var configuration = configurationBuilder.Build();
        var services = new ServiceCollection();

        services.AddDbContext<PlanTechDbContext>(opts =>
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

    public static void Main()
    {
        var provider = CreateServiceProvider();
        var seeder = provider.GetRequiredService<SeedData>();
        seeder.CreateData();
    }
}
