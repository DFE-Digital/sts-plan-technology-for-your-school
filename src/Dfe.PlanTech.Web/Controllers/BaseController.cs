using System.Diagnostics;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController> : Controller
{
    protected readonly ICacher cacher;
    protected readonly ILogger<TConcreteController> logger;

    public BaseController(ICacher cacher, ILogger<TConcreteController> logger)
    {
        this.cacher = cacher;
        this.logger = logger;
    }

    /// <summary>
    /// Gets last Visited URL for user from <see chref="UrlHistoryMiddleware"/>
    /// </summary>
    /// <returns></returns>
    protected string GetLastVisitedUrl()
    {
        var pageHistory = cacher.Get<Stack<string>>(UrlHistoryMiddleware.CACHE_KEY)!;
        if (pageHistory != null && pageHistory.TryPeek(out string? lastVisitedPage))
        {
            return lastVisitedPage ?? "";
        }

        return "";
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}