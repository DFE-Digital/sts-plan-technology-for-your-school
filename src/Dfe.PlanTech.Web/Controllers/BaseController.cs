using System.Diagnostics;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
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
        var history = new UrlHistory(cacher);
        var pageHistory = history.History;
        
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