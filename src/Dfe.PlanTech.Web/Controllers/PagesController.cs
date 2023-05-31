using System.Diagnostics;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : Controller
{
    private readonly ILogger<PagesController> _logger;

    public PagesController(ILogger<PagesController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/{route?}")]
    public async Task<IActionResult> GetByRoute(string? route, [FromServices] GetPageQuery query)
    {
        var slug = string.IsNullOrEmpty(route) ? Request.Path.Value : route;

        var page = await query.GetPageBySlug(slug);

        return View("Page", page);

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}