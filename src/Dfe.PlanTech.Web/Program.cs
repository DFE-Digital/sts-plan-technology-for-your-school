using Azure.Identity;
using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Infrastructure.SignIns;
using Dfe.PlanTech.Web;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using GovUk.Frontend.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITelemetryInitializer, CustomRequestDimensionsTelemetryInitializer>();

builder.Services.AddGoogleTagManager();
builder.Services.AddCspConfiguration();
// Add services to the container.

builder.Services.AddControllersWithViews();

builder.Services.AddGovUkFrontend();

if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = $"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/";
    var azureCredentials = new DefaultAzureCredential();

    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), azureCredentials);

    builder.Services.AddDbContext<DataProtectionDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<DataProtectionDbContext>()
        .ProtectKeysWithAzureKeyVault(new Uri(keyVaultUri + "keys/dataprotection"), azureCredentials);

    //Add overrides json for overwriting KV values for testing
    if (builder.Environment.IsStaging())
    {
        builder.Configuration.AddJsonFile("overrides.json", true);
    }
}

builder.AddContentAndSupportServices()
        .AddAuthorisationServices()
        .AddCaching()
        .AddContentfulServices(builder.Configuration)
        .AddCQRSServices()
        .AddDatabase(builder.Configuration)
        .AddDfeSignIn(builder.Configuration)
        .AddExceptionHandlingServices()
        .AddRoutingServices();

builder.Services.AddScoped<ComponentViewsFactory>();
builder.Services.AddSingleton<ISystemTime, SystemTime>();

var app = builder.Build();

app.UseSecurityHeaders();

app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.Always
    });

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(context =>
    {
        var exceptionHandlerMiddleware = context.RequestServices.GetRequiredService<IExceptionHandlerMiddleware>();
        exceptionHandlerMiddleware.ContextRedirect(context);

        return Task.CompletedTask;
    });
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    pattern: "{controller=Pages}/{action=GetByRoute}/{id?}",
    name: "default"
);

await app.RunAsync();
