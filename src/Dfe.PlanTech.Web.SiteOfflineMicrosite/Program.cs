using Dfe.PlanTech.Web.SiteOfflineMicrosite.Middleware;
using GovUk.Frontend.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddGovUkFrontend();

var app = builder.Build();

app.UseSecurityHeaders();
app.UseStaticFiles();
app.UseRouting();

// Health check endpoint - returns 200 to keep infrastructure happy, but indicates maintenance mode
// Note that any health/readiness probes returning false may prevent traffic from reaching this microsite
app.MapGet("/health", () => Results.Ok(new
{
    status = "maintenance",
    healthy = true,
    message = "Maintenance microsite is operational",
    mainSiteAvailable = false,
    timestamp = DateTime.UtcNow
})).DisableAntiforgery();

// Handle HEAD requests to health endpoint
app.MapMethods("/health", new[] { "HEAD" }, () => Results.Ok()).DisableAntiforgery();

// Catch-all route - all paths and HTTP methods go to Home/Index
app.MapControllerRoute(
    name: "catchall",
    pattern: "{**catchall}",
    defaults: new { controller = "Home", action = "Index" });

app.Run();

public partial class Program { }

