using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.Controllers;

public class HomeController : Controller
{
    [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS")]
    public IActionResult Index()
    {
        Response.StatusCode = 503;
        Response.Headers["Retry-After"] = "3600"; // Suggest retry after 1 hour (in seconds)
        return View();
    }
}

