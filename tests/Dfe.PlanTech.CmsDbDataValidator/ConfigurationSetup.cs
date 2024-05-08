using System.Reflection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace Dfe.PlanTech.CmsDbDataValidator;

public static class ConfigurationSetup
{
    public static readonly IConfiguration Configuration = BuildConfiguration();

    public static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder();

        builder.SetBasePath(AppContext.BaseDirectory);

        return builder.AddAppSettings()
                      .AddUserSecrets<Program>()
                      .Build();
    }

    public static ConfigurationBuilder AddAppSettings(this ConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        return builder;
    }
}