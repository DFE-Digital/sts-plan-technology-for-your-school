using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Core;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public PagesController(ILogger<PagesController> logger, IUrlHistory history) : base(logger, history)
    {
    }

    [HttpGet("/{route?}")]
    public async Task<IActionResult> GetByRoute(string? route, [FromServices] GetPageQuery query)
    {
        string slug = GetSlug(route);

        var page = await query.GetPageBySlug(slug);

        return View("Page", page);
    }

    /// <summary>
    /// Returns either the entire slug for the URL (if not null/empty), or "/"
    /// </summary>
    /// <param name="route">Route slug from route binding</param>
    private string GetSlug(string? route) => (string.IsNullOrEmpty(route) ? Request.Path.Value : route) ?? "/";
}