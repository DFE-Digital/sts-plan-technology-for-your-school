using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Dfe.PlanTech.Web.Controllers;

public class BaseController<TConcreteController> : Controller
{
    protected readonly ILogger<TConcreteController> logger;
    protected readonly IUrlHistory history;

    protected const string PAGE_DEFAULT = "self-assessment";

    public BaseController(ILogger<TConcreteController> logger, IUrlHistory history)
    {
        this.logger = logger;
        this.history = history;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    public async Task<string> GetBackUrl() =>(await history.GetLastVisitedUrl())?.ToString() ?? PAGE_DEFAULT;
}