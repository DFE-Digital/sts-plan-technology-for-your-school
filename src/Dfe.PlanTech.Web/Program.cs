using Dfe.PlanTech.Application;
using Dfe.PlanTech.Data.Sql;
using Dfe.PlanTech.Infrastructure.ServiceBus;
using Dfe.PlanTech.Infrastructure.SignIn;
using Dfe.PlanTech.Web;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Middleware;
using GovUk.Frontend.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddScoped<MaintainUrlOnKeyNotFoundAttribute>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<MaintainUrlOnKeyNotFoundAttribute>();
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddReleaseServices(builder.Configuration);
}

if (builder.Environment.EnvironmentName != "E2E")
{
    builder.Services.AddDbWriterServices(builder.Configuration);
}

builder.Configuration.AddCommandLine(args);

builder.AddSystemConfiguration();
builder.AddContentAndSupportConfiguration();

builder.Services.AddGovUkFrontendConfiguration().AddGovUkFrontend().AddHttpContextAccessor();

builder
    .Services.AddAuthorisationServices()
    .AddCaching()
    .AddContentfulServices(builder.Configuration)
    .AddCookies(builder.Configuration)
    .AddCurrentUser()
    .AddCustomTelemetry()
    .AddDatabase(builder.Configuration)
    .AddDfeSignIn(builder.Configuration)
    .AddExceptionHandlingServices()
    .AddGoogleTagManager()
    .AddRoutingServices()
    .AddRedisServices(builder.Configuration)
    .AddRepositories()
    .AddViewComponents();

builder.Services.AddApplicationServices().AddApplicationWorkflows();

var app = builder.Build();

app.UseRobotsTxtMiddleware();

app.UseSecurityHeaders();
app.UseMiddleware<HeadRequestMiddleware>();

app.UseCookiePolicy(new CookiePolicyOptions { Secure = CookieSecurePolicy.Always });

app.UseForwardedHeaders();

app.UseMiddleware<ExploitPathMiddleware>();

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
        var exceptionHandlerMiddleware =
            context.RequestServices.GetRequiredService<IExceptionHandlerMiddleware>();
        await exceptionHandlerMiddleware.HandleExceptionAsync(context);
    });
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(pattern: "{controller=Pages}/{action=GetByRoute}/{id?}", name: "default");

await app.RunAsync();
