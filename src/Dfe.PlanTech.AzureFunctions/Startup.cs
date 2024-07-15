using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.PlanTech.AzureFunctions.Config;
using Dfe.PlanTech.AzureFunctions.Services;
using Dfe.PlanTech.AzureFunctions.Utils;

namespace Dfe.PlanTech.AzureFunctions
{
    [ExcludeFromCodeCoverage]
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CmsDbContext>(options =>
            {
                options.UseSqlServer(configuration["AZURE_SQL_CONNECTIONSTRING"]);
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

            services.AddSingleton(new ContentfulOptions(bool.Parse(configuration["Contentful:UsePreview"] ?? bool.FalseString)));
            services.AddSingleton(new CacheRefreshConfiguration(
                configuration["WEBSITE_CACHE_CLEAR_ENDPOINT"],
                configuration["WEBSITE_CACHE_CLEAR_APIKEY_NAME"],
                configuration["WEBSITE_CACHE_CLEAR_APIKEY_VALUE"]));
            services.AddHttpClient<CacheHandler>();
            services.AddTransient<ICacheHandler, CacheHandler>();

            services.AddOptions<MessageRetryHandlingOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("MessageRetryHandlingOptions").Bind(settings);
                });

            AddMappers(services);
            AddMessageRetryHandler(services);
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