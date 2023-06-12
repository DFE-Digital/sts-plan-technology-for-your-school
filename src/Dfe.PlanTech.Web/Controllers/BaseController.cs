using System.Diagnostics;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController> : Controller
{
    protected readonly ILogger<TConcreteController> logger;
    protected readonly IUrlHistory history;

    public BaseController(ILogger<TConcreteController> logger, IUrlHistory history)
    {
        this.logger = logger;
        this.history = history;
    }

    /// <summary>
    /// Gets last Visited URL for user from <see chref="UrlHistoryMiddleware"/>
    /// </summary>
    /// <returns></returns>
    protected string GetLastVisitedUrl()
    {
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