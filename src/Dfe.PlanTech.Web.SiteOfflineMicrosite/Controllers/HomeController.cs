using Dfe.PlanTech.Web.SiteOfflineMicrosite.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.Controllers;

public class HomeController : Controller
{
    private readonly MaintenanceConfiguration _config;

    public HomeController(IOptions<MaintenanceConfiguration> config)
    {
        _config = config.Value;
    }

    [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS")]
    public IActionResult Index()
    {
        Response.StatusCode = 503;
        Response.Headers.RetryAfter = "3600"; // Suggest retry after 1 hour (in seconds)
        return View(_config);
    }
}

