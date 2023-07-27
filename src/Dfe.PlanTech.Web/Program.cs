using Azure.Identity;
using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Infrastructure.SignIn;
using Dfe.PlanTech.Web;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddGoogleTagManager(builder.Configuration);
// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddGovUkFrontend();

if (builder.Environment.IsProduction())
{
    var keyVaultUri = $"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/";
    var azureCredentials = new DefaultAzureCredential();

    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), azureCredentials);

    builder.Services.AddDbContext<DataProtectionDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
    builder.Services.AddDataProtection()
                        .PersistKeysToDbContext<DataProtectionDbContext>()
                        .ProtectKeysWithAzureKeyVault(new Uri(keyVaultUri + "keys/dataprotection"), azureCredentials);
}

builder.Services.AddCaching(builder.Configuration);
builder.Services.AddCQRSServices();
builder.Services.AddContentfulServices(builder.Configuration);
builder.Services.AddDfeSignIn(builder.Configuration);
builder.Services.AddScoped<ComponentViewsFactory>();
builder.Services.AddJsonOptions();

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UrlHistoryMiddleware>();

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
    pattern: "{controller=Pages}/{action=GetByRoute}/{id?}",
    name: "default"
);


app.MapControllerRoute(
    name: "recommendationsController",
    pattern: "recommendations/"
);

app.Run();