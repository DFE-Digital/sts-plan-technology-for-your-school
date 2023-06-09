using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Web.Helpers;
using GovUk.Frontend.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddGovUkFrontend();

builder.Services.SetupContentfulClient(builder.Configuration, "Contentful", HttpClientPolicyExtensions.AddRetryPolicy);

builder.Services.AddScoped((_) => new TextRendererOptions(new List<MarkOption>() {
    new MarkOption(){
        Mark = "bold",
        HtmlTag = "span",
        Classes = "govuk-body govuk-!-font-weight-bold",
    }
}));

builder.Services.AddScoped((_) => new ParagraphRendererOptions()
{
    Classes = "govuk-body",
});

builder.Services.AddScoped((_) => new HyperlinkRendererOptions()
{
    Classes = "govuk-link",
});

builder.Services.AddScoped<ComponentViewsFactory>();

builder.Services.AddCQRSServices();


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".Dfe.PlanTech";
});

builder.Services.AddSingleton<ICacheOptions>((services) => new CacheOptions());
builder.Services.AddScoped<ICacher, Cacher>();

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

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "questionsController",
    pattern: "question/{action=GetQuestionById}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pages}/{action=GetByRoute}/{id?}");

app.Run();