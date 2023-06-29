using Azure.Identity;
using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Web;
using Dfe.PlanTech.Infrastructure.SignIn;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using GovUk.Frontend.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddGovUkFrontend();

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());
}

builder.Services.AddCaching();
builder.Services.AddCQRSServices();
builder.Services.AddContentfulServices(builder.Configuration);
builder.Services.AddDfeSignIn(builder.Configuration);
builder.Services.AddScoped<ComponentViewsFactory>();

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseForwardedHeaders();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UrlHistoryMiddleware>();

app.MapControllerRoute(
    name: "questionsController",
    pattern: "question/{action=GetQuestionById}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pages}/{action=GetByRoute}/{id?}");

app.Run();