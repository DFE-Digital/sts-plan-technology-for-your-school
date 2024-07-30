using Dfe.PlanTech.AzureFunctions;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        Startup.ConfigureServices(services, context.Configuration);
    })
    .Build();

await host.RunAsync();
