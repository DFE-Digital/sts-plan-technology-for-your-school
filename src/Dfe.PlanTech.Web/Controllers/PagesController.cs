using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Dfe.PlanTech.Application.Users.Interfaces;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public PagesController(ILogger<PagesController> logger) : base(logger)
    {
    }

    [Authorize(Policy = PageModelAuthorisationPolicy.POLICY_NAME)]
    [HttpGet("/{route?}")]
    [Route("~/{SectionSlug}/recommendation/{route?}", Name = "GetPageByRouteAndSection")]
    public IActionResult GetByRoute(string route, [ModelBinder(typeof(PageModelBinder))] Page page, [FromServices] IUser user)
    {
        string slug = GetSlug(route);
        string param = "";
        TempData["SectionSlug"] = route;

        if (TempData[slug] is string tempDataSlug) param = tempDataSlug;

        if (!string.IsNullOrEmpty(param))
            TempData["Param"] = param;

        var viewModel = CreatePageModel(page, param, user);

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

    private PageViewModel CreatePageModel(Page page, string param, IUser user)
    {
        ViewData["Title"] = page.Title?.Text ?? "Plan Technology For Your School";

        var viewModel = new PageViewModel()
        {
            Page = page,
            Param = param
        };

        viewModel.TryLoadOrganisationName(HttpContext, user, logger);

        return viewModel;
    }

    /// <summary>
    /// Returns either the entire slug for the URL (if not null/empty), or "/"
    /// </summary>
    /// <param name="route">Route slug from route binding</param>
    private string GetSlug(string? route) => (string.IsNullOrEmpty(route) ? Request.Path.Value : route) ?? "/";
}