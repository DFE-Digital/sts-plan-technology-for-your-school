using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public PagesController(ILogger<PagesController> logger, IUrlHistory history) : base(logger, history)
    {
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken, [FromServices] GetPageQuery query)
    {
        var page = await query.GetPageBySlug("/", cancellationToken);

        var viewModel = CreatePageModel(page);

        return View("Page", viewModel);
    }

    [Authorize]
    [HttpGet("/{route?}")]
    public async Task<IActionResult> GetByRoute(string route, CancellationToken cancellationToken, [FromServices] GetPageQuery query)
    {
        string slug = GetSlug(route);

        var page = await query.GetPageBySlug(slug, cancellationToken);

        var viewModel = CreatePageModel(page);

        return View("Page", viewModel);
    }

    private PageViewModel CreatePageModel(Page page)
    {
        return new PageViewModel()
        {
            Page = page,
            BackUrl = history.LastVisitedUrl?.ToString() ?? "/"
        };
    }

    /// <summary>
    /// Returns either the entire slug for the URL (if not null/empty), or "/"
    /// </summary>
    /// <param name="route">Route slug from route binding</param>
    private string GetSlug(string? route) => (string.IsNullOrEmpty(route) ? Request.Path.Value : route) ?? "/";
}