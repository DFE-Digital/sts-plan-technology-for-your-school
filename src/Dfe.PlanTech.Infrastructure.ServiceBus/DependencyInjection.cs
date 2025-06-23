using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Persistence.Commands;
using Dfe.PlanTech.Core.Persistence.Interfaces;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Commands;
using Dfe.PlanTech.Infrastructure.ServiceBus.Queues;
using Dfe.PlanTech.Infrastructure.ServiceBus.Results;
using Dfe.PlanTech.Infrastructure.ServiceBus.Retries;
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
            .AddMessageRetryHandler();

        services.AddTransient<ICmsWebHookMessageProcessor, CmsWebHookMessageProcessor>();
        services.AddSingleton(new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        });

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

        services.Configure<ServiceBusOptions>(configuration.GetSection(nameof(ServiceBusOptions)));
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
}
