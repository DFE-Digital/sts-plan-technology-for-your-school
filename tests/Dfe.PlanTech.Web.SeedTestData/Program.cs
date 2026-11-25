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
            opts.UseSqlServer("Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=Pa5ssw0rd@G0esH3r3~;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=testDb");
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
