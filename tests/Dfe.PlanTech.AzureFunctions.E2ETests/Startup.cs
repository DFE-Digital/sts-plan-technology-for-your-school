using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.AzureFunctions.E2ETests;

public static class Startup
{
  public static IConfiguration Configure()
  {
    var configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.AddUserSecrets(typeof(Startup).Assembly);

    return configurationBuilder.Build();
  }

  public static ServiceProvider BuildServiceProvider(IConfiguration? configuration = null)
  {
    configuration ??= Configure();

    var services = new ServiceCollection();
    services.AddTransient((services) => new ContentfulOptions(true));
    services.AddEntityFrameworkSqlServer();
    services.AddDbContext<CmsDbContext>((options) =>
    {
      options.UseSqlServer(configuration["ConnectionStrings:Database"]);
    });

    var provider = services.BuildServiceProvider();

    return provider;
  }
}