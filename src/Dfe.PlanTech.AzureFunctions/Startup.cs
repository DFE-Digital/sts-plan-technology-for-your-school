using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.AzureFunctions
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CmsDbContext>(options => options.UseSqlServer(configuration["AZURE_SQL_CONNECTIONSTRING"]));

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

            AddMappers(services);
        }

        /// <summary>
        /// Finds all <see cref="JsonToDbMapper"/> mappers using reflection, and then injects them as dependencies 
        /// </summary>
        /// <param name="services"></param>
        private static void AddMappers(IServiceCollection services)
        {
            foreach (var mapper in GetMappers())
            {
                services.AddScoped(typeof(JsonToDbMapper), mapper);
            }

            services.AddScoped<RichTextContentMapper>();

            services.AddScoped<JsonToEntityMappers>();
        }

        /// <summary>
        /// Get all <see cref="JsonToDbMapper"/> mappers using reflection 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetMappers() => Assembly.GetEntryAssembly()!.GetTypes().Where(type => type.IsAssignableTo(typeof(JsonToDbMapper)) && !type.IsAbstract);
    }
}
