using Azure.Identity;
using Dfe.PlanTech.AzureFunctions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, builder) =>
    {
        var configuration = builder.Build();
        var keyvaultName = configuration["Azure_KeyVault_Name"];

        //If there's no Key Vault name provided then don't add it as a configuration provider - For local testing
        if (string.IsNullOrEmpty(keyvaultName))
        {
            Console.WriteLine($"Couldn't find Key Vault name with key Azure_KeyVault_Name");
            return;
        }

        var keyVaultUri = new Uri($"https://{keyvaultName}.vault.azure.net");
        var keyVaultCredential = new DefaultAzureCredential();
        builder.AddAzureKeyVault(keyVaultUri, keyVaultCredential);
    })
    .ConfigureServices((context, services) =>
    {
        services.ConfigureServices(context.Configuration);
    })
    .Build();

await host.RunAsync();
