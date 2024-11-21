using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Infrastructure.ServiceBus;
using Dfe.PlanTech.Infrastructure.SignIns;
using Dfe.PlanTech.Web.Configuration;
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
builder.Services.Configure<ErrorMessages>(builder.Configuration.GetSection("ErrorMessages"));
builder.Services.Configure<ContactOptions>(builder.Configuration.GetSection("ContactUs"));

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
        .AddRoutingServices()
        .AddRedisServices(builder.Configuration);

builder.Services.AddSingleton<ISystemTime, SystemTime>();

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

await app.RunAsync();
