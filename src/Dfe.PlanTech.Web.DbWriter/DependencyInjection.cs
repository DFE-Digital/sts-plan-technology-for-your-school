using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Caching.Services;
using Dfe.PlanTech.Application.Persistence.Commands;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Web.DbWriter.Retry;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Web.DbWriter;

public static class DependencyInjection
{
  /// <summary>
  /// Adds required services for the CMS->DB process
  /// </summary>
  /// <param name="services"></param>
  /// <param name="configuration"></param>
  /// <returns></returns>
  public static IServiceCollection AddDbWriterServices(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddServiceBusServices(configuration)
            .AddCaching()
            .AddMessageRetryHandler()
            .AddMappers();

    services.AddTransient<WebhookToDbCommand>();
    services.AddSingleton(new JsonSerializerOptions()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    });

    services.AddTransient<IDatabaseHelper<ICmsDbContext>, DatabaseHelper<ICmsDbContext>>();
    return services;
  }

  private static IServiceCollection AddServiceBusServices(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddAzureClients(builder =>
    {
      builder.AddServiceBusClient(configuration.GetConnectionString("servicebusconnection"));

      builder.AddClient<ServiceBusProcessor, ServiceBusClientOptions>((_, _, provider) => provider.GetService<ServiceBusClient>()!.CreateProcessor("contentful", new ServiceBusProcessorOptions() { PrefetchCount = 10 })).WithName("contentfulprocessor");
      builder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) => provider.GetService<ServiceBusClient>()!.CreateSender("contentful")).WithName("contentfulsender");

      builder.UseCredential(new DefaultAzureCredential());
    });

    services.AddHostedService<ContentfulServiceBusProcessor>();
    services.AddTransient<ServiceBusResultProcessor>();
    services.AddTransient<IMessageRetryHandler, MessageRetryHandler>();

    services.AddSingleton(new ServiceBusOptions()
    {
      MessagesPerBatch = 10,
    });
    return services;
  }

  private static IServiceCollection AddCaching(this IServiceCollection services)
  {
    services.AddOptions<CacheRefreshConfiguration>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
              configuration.GetSection("CacheClear").Bind(settings);
            });

    services.AddHttpClient<CacheHandler>()
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
        {
          PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        });

    services.AddTransient<ICacheHandler, CacheHandler>();

    return services;
  }

  /// <summary>
  /// Finds all <see cref="JsonToDbMapper"/> mappers using reflection, and then injects them as dependencies
  /// </summary>
  /// <param name="services"></param>
  private static IServiceCollection AddMappers(this IServiceCollection services)
  {
    var mappers = GetMappers().ToList();
    var otherMappers = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.IsAssignableFrom(typeof(JsonToDbMapper)) && !type.IsAbstract);

    foreach (var mapper in GetMappers())
    {
      services.AddTransient(typeof(JsonToDbMapper), mapper);
    }

    services.AddTransient<RichTextContentMapper>();
    services.AddTransient<JsonToEntityMappers>();

    services.AddTransient<PageEntityRetriever>();
    services.AddTransient<PageEntityUpdater>();

    services.AddTransient<EntityUpdater>();

    return services;
  }

  private static IServiceCollection AddMessageRetryHandler(this IServiceCollection services)
  {
    services.AddOptions<MessageRetryHandlingOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
              configuration.GetSection("MessageRetryHandlingOptions").Bind(settings);
            });

    services.AddTransient<IMessageRetryHandler, MessageRetryHandler>();
    return services;
  }

  /// <summary>
  /// Get all <see cref="JsonToDbMapper"/> mappers using reflection
  /// </summary>
  /// <returns></returns>
  private static IEnumerable<Type> GetMappers() =>
    AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.IsAssignableTo(typeof(JsonToDbMapper)) && !type.IsAbstract);

}
