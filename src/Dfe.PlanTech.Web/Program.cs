using Dfe.PlanTech.Application.Helpers;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Helpers;
using Dfe.PlanTech.Web.Helpers;
using GovUk.Frontend.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddGovUkFrontend();

builder.Services.SetupContentfulClient(builder.Configuration, "Contentful");

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