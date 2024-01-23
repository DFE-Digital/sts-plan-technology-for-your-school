using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace Dfe.PlanTech.DataValidation.Tests;

public static class ConfigurationSetup
{
  public static readonly IConfiguration Configuration = BuildConfiguration();

  public static IConfiguration BuildConfiguration()
  {
    var builder = new ConfigurationBuilder();

    builder.SetBasePath(AppContext.BaseDirectory);

    return builder.AddAppSettings()
                  .AddUserSecrets<CachedDataComparisonTests>()
                  .Build();
  }

  private static ConfigurationBuilder AddKeyVault(this ConfigurationBuilder builder)
  {
    string? keyVaultEndpoint = GetKeyVaultEndPoint(builder);

    var azureServiceTokenProvider = new AzureServiceTokenProvider();
    var keyVaultClient = new KeyVaultClient(
        new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

    builder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());

    return builder;
  }

  private static string? GetKeyVaultEndPoint(ConfigurationBuilder builder)
  {
    var configuration = builder.Build();
    var keyVaultEndpoint = configuration.GetConnectionString("KeyVaultEndpoint");
    return keyVaultEndpoint ?? throw new ArgumentException("Key Vault Endpoint not found in configuration");
  }

  public static ConfigurationBuilder AddAppSettings(this ConfigurationBuilder builder)
  {
    builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    return builder;
  }
}