using AutoMapper;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.Mappings;
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
      services.AddDbContext<CmsDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Database")));

      services.AddAzureClients(builder =>
      {
        builder.AddServiceBusClient(configuration["AzureWebJobsServiceBus"]);

        builder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
                provider.GetService<ServiceBusClient>()!.CreateSender("contentful")
            )
            .WithName("contentful");

        builder.UseCredential(new DefaultAzureCredential());
      });

      var config = new MapperConfiguration(cfg =>
      {
        cfg.AddProfile<CmsDbProfile>();
      });

      var mapper = config.CreateMapper();
      services.AddTransient((services) => config.CreateMapper());
    }
  }
}
