using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Infrastructure.Redis;
using Dfe.PlanTech.Infrastructure.ServiceBus;
using Dfe.PlanTech.Infrastructure.SignIns;
using Dfe.PlanTech.Web;
using Dfe.PlanTech.Web.Middleware;
using GovUk.Frontend.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddControllersWithViews();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddReleaseServices(builder.Configuration);
}

if (builder.Environment.EnvironmentName != "E2E")
{
    builder.Services.AddDbWriterServices(builder.Configuration);
}

builder.Services.AddCustomTelemetry();

builder.AddContentAndSupportServices()
        .AddAuthorisationServices()
        .AddCaching()
        .AddContentfulServices(builder.Configuration)
        .AddCQRSServices()
        .AddCspConfiguration()
        .AddDatabase(builder.Configuration)
        .AddDfeSignIn(builder.Configuration)
        .AddExceptionHandlingServices()
        .AddGoogleTagManager()
        .AddGovUkFrontend()
        .AddHttpContextAccessor()
        .AddRoutingServices();

builder.Services.AddSingleton<ISystemTime, SystemTime>();

// ----------------
// KD: it should be as straightforward as creating a redis instance, putting the connection string in secrets and refing here. It's designed to be singleton.
// Just inject IDistCache where you need it
// ----------------

//TODO: Different exception type
builder.Services.AddSingleton(
    new DistributedCachingOptions(ConnectionSting: builder.Configuration.GetConnectionString("redis") ?? throw new Exception("Redis connection string is empty!")));
builder.Services.AddSingleton<IDistributedCache, RedisCache>();
builder.Services.AddSingleton<IDistributedLockProvider, RedisLockProvider>();

var app = builder.Build();

app.UseSecurityHeaders();
app.UseMiddleware<HeadRequestMiddleware>();

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

await app.Services.GetRequiredService<IDistCache>().InitialiseAsync();

await app.RunAsync();
