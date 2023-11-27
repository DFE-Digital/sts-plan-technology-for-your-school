using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Dfe.PlanTech.AzureFunctions.Startup))]

namespace Dfe.PlanTech.AzureFunctions
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      var configuration = builder.GetContext().Configuration ?? throw new NullReferenceException("Configuration should not be null");

      ConfigureServices(builder.Services, configuration);
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
      services.AddDbContext<CmsDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Database")));

      services.AddAzureClients(builder =>
      {
        builder.AddServiceBusClient("CONNECTION STRING");
      });
    }
  }
}
