using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Web;

[ExcludeFromCodeCoverage]
public static class ReleaseProgramExtensions
{
    public static IServiceCollection AddReleaseServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        var keyVaultUri = $"https://{configuration["KeyVaultName"]}.vault.azure.net/";
        var azureCredentials = new DefaultAzureCredential();
        configuration.AddAzureKeyVault(new Uri(keyVaultUri), azureCredentials);

        AddDataProtection(services, configuration, keyVaultUri, azureCredentials);

        return services;
    }

    public static void AddDataProtection(IServiceCollection services, ConfigurationManager configuration, string keyVaultUri, DefaultAzureCredential azureCredentials)
    {
        services.AddDbContext<DataProtectionDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Database")));
        services.AddDataProtection()
                .PersistKeysToDbContext<DataProtectionDbContext>()
                .ProtectKeysWithAzureKeyVault(new Uri(keyVaultUri + "keys/dataprotection"), azureCredentials);
    }
}