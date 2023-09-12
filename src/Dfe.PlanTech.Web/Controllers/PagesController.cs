using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public PagesController(ILogger<PagesController> logger) : base(logger)
    {
    }

    [Authorize(Policy = "UsePageAuthentication")]
    [HttpGet("/{route?}")]
    public IActionResult GetByRoute(string route, [ModelBinder(typeof(PageModelBinder))] Page jimTesting)
    {
        string slug = GetSlug(route);
        string param = "";

        if (TempData[slug] is string tempDataSlug) param = tempDataSlug;

        if (!string.IsNullOrEmpty(param))
            TempData["Param"] = param;

        var page = HttpContext.Items["page"] as Page ?? throw new KeyNotFoundException("Could not find HttpContext item for Page");

        var viewModel = CreatePageModel(page!, param);

        return View("Page", viewModel);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/error")]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    [Route("/service-unavailable")]
    public IActionResult ServiceUnavailable()
    {
        return View(new ServiceUnavailableViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private PageViewModel CreatePageModel(Page page, string param = null!)
    {
        ViewData["Title"] = page.Title?.Text ?? "Plan Technology For Your School";

        return new PageViewModel()
        {
            Page = page,
            Param = param,
        };
    }

    /// <summary>
    /// Returns either the entire slug for the URL (if not null/empty), or "/"
    /// </summary>
    /// <param name="route">Route slug from route binding</param>
    private string GetSlug(string? route) => (string.IsNullOrEmpty(route) ? Request.Path.Value : route) ?? "/";
}