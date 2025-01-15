using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Infrastructure.ServiceBus;
using Dfe.PlanTech.Infrastructure.SignIns;
using Dfe.PlanTech.Web;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Models;
using GovUk.Frontend.AspNetCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.MaxDepth = 128;
    });

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
builder.Services.Configure<ErrorPages>(builder.Configuration.GetSection("ErrorPages"));
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
builder.Services.Configure<RobotsConfiguration>(builder.Configuration.GetSection("Robots"));


var app = builder.Build();

app.UseRobotsTxtMiddleware();

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
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerMiddleware = context.RequestServices.GetRequiredService<IExceptionHandlerMiddleware>();
        var pageQuery = context.RequestServices.GetRequiredService<IGetPageQuery>();
        var errorPages = context.RequestServices.GetRequiredService<IOptions<ErrorPages>>();

        var internalErrorPage = await pageQuery.GetPageById(errorPages.Value.InternalErrorPageId);
        var internalErrorSlug = internalErrorPage?.Slug ?? UrlConstants.Error;

        exceptionHandlerMiddleware.ContextRedirect(internalErrorSlug, context);
    });
});

app.UseStaticFiles();
app.UseResponseCompression();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    pattern: "{controller=Pages}/{action=GetByRoute}/{id?}",
    name: "default"
);

await app.RunAsync();
