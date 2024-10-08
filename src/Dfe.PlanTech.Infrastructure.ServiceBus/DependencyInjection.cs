using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Content.Commands;
using Dfe.PlanTech.Application.Persistence.Commands;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Application.Queues.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Infrastructure.ServiceBus.Results;
using Dfe.PlanTech.Infrastructure.ServiceBus.Retry;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.ServiceBus;

[ExcludeFromCodeCoverage]
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
                .AddMessageRetryHandler()
                .AddMappers();

        services.AddTransient<IWebhookToDbCommand, WebhookToDbCommand>();
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
            builder.AddServiceBusClient(configuration.GetConnectionString("ServiceBus"));

            builder.AddClient<ServiceBusProcessor, ServiceBusClientOptions>((_, _, provider) => provider.GetService<ServiceBusClient>()!.CreateProcessor("contentful", new ServiceBusProcessorOptions() { PrefetchCount = 10 })).WithName("contentfulprocessor");
            builder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) => provider.GetService<ServiceBusClient>()!.CreateSender("contentful")).WithName("contentfulsender");

            builder.UseCredential(new DefaultAzureCredential());
        });

        services.AddHostedService<ContentfulServiceBusProcessor>();
        services.AddTransient<IServiceBusResultProcessor, ServiceBusResultProcessor>();
        services.AddTransient<IMessageRetryHandler, MessageRetryHandler>();
        services.AddTransient<IQueueWriter, QueueWriter>();
        services.AddTransient<IWriteCmsWebhookToQueueCommand, WriteCmsWebhookToQueueCommand>();

        services.AddSingleton(new ServiceBusOptions() { MessagesPerBatch = 10 });
        return services;
    }

    /// <summary>
    /// Finds all <see cref="BaseJsonToDbMapper"/> mappers using reflection, and then injects them as dependencies
    /// </summary>
    /// <param name="services"></param>
    private static IServiceCollection AddMappers(this IServiceCollection services)
    {
        foreach (var mapper in GetMappers())
        {
            services.AddTransient(typeof(BaseJsonToDbMapper), mapper);
        }

        services.AddTransient<RichTextContentMapper>();
        services.AddTransient<JsonToEntityMappers>();

        services.AddTransient<PageRetriever>();
        services.AddTransient<PageUpdater>();

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
    /// Get all <see cref="BaseJsonToDbMapper"/> mappers using reflection
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<Type> GetMappers() =>
      AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(assembly => assembly.GetTypes())
          .Where(type => type.IsAssignableTo(typeof(BaseJsonToDbMapper)) && !type.IsAbstract);

}
