using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Config;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Services;
using Dfe.PlanTech.AzureFunctions.Utils;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.AzureFunctions
{
    [ExcludeFromCodeCoverage]
    public static class Startup
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CmsDbContext>(options =>
            {
                options.UseSqlServer(configuration["ConnectionStrings:Database"]);
            });

            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(configuration["AzureWebJobsServiceBus"]);

                builder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
                        provider.GetService<ServiceBusClient>()!.CreateSender("contentful")
                    )
                    .WithName("contentful");

                builder.UseCredential(new DefaultAzureCredential());
            });

            services.AddSingleton(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            });

            ConfigureCaching(services);
            ConfigureContentful(services);
            ConfigureRetryHandling(services);

            AddMappers(services);
            AddMessageRetryHandler(services);
        }

        private static void ConfigureRetryHandling(IServiceCollection services)
        {
            services.AddOptions<MessageRetryHandlingOptions>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("MessageRetryHandlingOptions").Bind(settings);
                    });
        }

        private static void ConfigureContentful(IServiceCollection services)
        {
            services.AddOptions<ContentfulOptions>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("Contentful").Bind(settings);
                    });

            services.AddTransient(services => services.GetRequiredService<IOptions<ContentfulOptions>>().Value);
        }

        private static void ConfigureCaching(IServiceCollection services)
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
        }

        /// <summary>
        /// Finds all <see cref="JsonToDbMapper"/> mappers using reflection, and then injects them as dependencies 
        /// </summary>
        /// <param name="services"></param>
        private static void AddMappers(IServiceCollection services)
        {
            foreach (var mapper in GetMappers())
            {
                services.AddTransient(typeof(JsonToDbMapper), mapper);
            }

            services.AddTransient<RichTextContentMapper>();
            services.AddTransient<JsonToEntityMappers>();

            services.AddTransient<PageEntityRetriever>();
            services.AddTransient<PageEntityUpdater>();

            services.AddTransient<EntityRetriever>();
            services.AddTransient<EntityUpdater>();
        }

        private static void AddMessageRetryHandler(IServiceCollection services)
        {
            services.AddTransient<IMessageRetryHandler, MessageRetryHandler>();
        }

        /// <summary>
        /// Get all <see cref="JsonToDbMapper"/> mappers using reflection 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetMappers() => Assembly.GetEntryAssembly()!.GetTypes()
            .Where(type => type.IsAssignableTo(typeof(JsonToDbMapper)) && !type.IsAbstract);
    }
}
