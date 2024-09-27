using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests;

public static class Startup
{
    public static IServiceProvider CreateServiceProvider(IConfiguration? configuration = null)
    {
        configuration ??= CreateConfiguration();

        var services = new ServiceCollection();

        services.AddDbContext<CmsDbContext>(opts =>
        {
            opts.UseSqlServer(configuration.GetConnectionString("Database"));
            opts.EnableSensitiveDataLogging(true);
        });

        services.AddLogging(opts =>
        {
            opts.AddConsole();
            opts.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddSingleton(new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true,
        });

        services.AddSingleton(new ContentfulOptions(true));

        services.AddOptions<MessageRetryHandlingOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("MessageRetryHandlingOptions").Bind(settings);
            });

        AddMappers(services);

        services.AddTransient<IDatabaseHelper<ICmsDbContext>>();
        return services.BuildServiceProvider();
    }

    private static void AddMappers(IServiceCollection services)
    {
        foreach (var mapper in GetMappers())
        {
            services.AddScoped(typeof(JsonToDbMapper), mapper);
        }

        services.AddScoped<RichTextContentMapper>();
        services.AddScoped<JsonToEntityMappers>();

        services.AddTransient<PageRetriever>();
        services.AddTransient<PageUpdater>();

        services.AddTransient<EntityUpdater>();
    }

    private static IEnumerable<Type> GetMappers()
     => AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(assembley => assembley.GetTypes())
                                .Where(type => type.IsAssignableTo(typeof(JsonToDbMapper)) && !type.IsAbstract);

    public static IConfiguration CreateConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddUserSecrets<ForLoadingUserSecrets>();

        return configurationBuilder.Build();
    }
}

public class ForLoadingUserSecrets
{

}
