using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
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

    [HttpGet("/")]
    public async Task<IActionResult> Index([FromServices] GetPageQuery query, CancellationToken cancellationToken)
    {
        var page = await query.GetPageBySlug("/", cancellationToken);

        var viewModel = CreatePageModel(page);

        return View("Page", viewModel);
    }

    [Authorize]
    [HttpGet("/{route?}")]
    public async Task<IActionResult> GetByRoute(string route, [FromServices] GetPageQuery query, CancellationToken cancellationToken)
    {
        string slug = GetSlug(route);
        string param = "";

        if (TempData[slug] is string tempDataSlug) param = tempDataSlug;

        if (!string.IsNullOrEmpty(param))
            TempData["Param"] = param;

        var page = await query.GetPageBySlug(slug, cancellationToken);

        var viewModel = CreatePageModel(page, param);

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