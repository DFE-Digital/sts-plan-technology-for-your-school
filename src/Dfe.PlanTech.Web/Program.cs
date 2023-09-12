using Azure.Identity;
using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Infrastructure.SignIn;
using Dfe.PlanTech.Web;
using Dfe.PlanTech.Web.Exceptions;
using Dfe.PlanTech.Web.Helpers;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddGoogleTagManager(builder.Configuration);
// Add services to the container.

builder.Services.AddControllersWithViews(options =>
{
});
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


builder.Services.AddCaching();
builder.Services.AddCQRSServices();
builder.Services.AddDfeSignIn(builder.Configuration);
builder.Services.AddScoped<ComponentViewsFactory>();

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddSingleton<IAuthorizationHandler, AuthorisationPolicy>();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UsePageAuthentication", policy =>
    {
        policy.Requirements.Add(new PageAuthorisationRequirement());
    });
});


builder.Services.AddContentfulServices(builder.Configuration);

var app = builder.Build();

app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.Always
    });
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

        var error = exceptionHandlerPathFeature?.Error;

        if (error is ContentfulDataUnavailableException)
        {
            context.Response.Redirect("/service-unavailable");
        }
        return Task.CompletedTask;
    });
});

app.UseCookiePolicy();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "questionsController",
    pattern: "question/{action=GetQuestionById}/{id?}");

app.MapControllerRoute(
    name: "checkAnswersController",
    pattern: "check-answers/"
);

app.MapControllerRoute(
    name: "checkAnswersController",
    pattern: "change-answer/"
);

app.MapControllerRoute(
    name: "checkAnswersController",
    pattern: "confirm-check-answers/"
);

app.MapControllerRoute(
    name: "recommendationsController",
    pattern: "recommendations/"
);

app.MapControllerRoute(
    pattern: "{controller=Pages}/{action=GetByRoute}/{id?}",
    name: "default"
);

app.Run();