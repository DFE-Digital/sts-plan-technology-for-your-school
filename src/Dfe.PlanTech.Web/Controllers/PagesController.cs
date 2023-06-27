using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public PagesController(ILogger<PagesController> logger, IUrlHistory history) : base(logger, history)
    {
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index([FromServices] GetPageQuery query, CancellationToken cancellationToken)
    {
        var page = await query.GetPageBySlug("/", cancellationToken);

        return View("Page", page);
    }

    [Authorize]
    [HttpGet("/{route?}")]
    public async Task<IActionResult> GetByRoute(string route, [FromServices] GetPageQuery query, CancellationToken cancellationToken)
    {
        string slug = GetSlug(route);

        var page = await query.GetPageBySlug(slug, cancellationToken);

        return View("Page", page);
    }

    /// <summary>
    /// Returns either the entire slug for the URL (if not null/empty), or "/"
    /// </summary>
    /// <param name="route">Route slug from route binding</param>
    private string GetSlug(string? route) => (string.IsNullOrEmpty(route) ? Request.Path.Value : route) ?? "/";
}