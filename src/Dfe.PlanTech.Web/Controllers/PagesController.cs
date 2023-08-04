using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public IConfiguration Config { get; }

    public PagesController(ILogger<PagesController> logger, IConfiguration config) : base(logger)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
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
    public async Task<IActionResult> GetByRoute(string route, [FromServices] GetPageQuery query, CancellationToken cancellationToken, string param = "")
    {
        string slug = GetSlug(route);
        if (!string.IsNullOrEmpty(param))
            TempData["Param"] = param;

        var page = await query.GetPageBySlug(slug, cancellationToken);

        var viewModel = CreatePageModel(page, param);

        return View("Page", viewModel);
    }

    private PageViewModel CreatePageModel(Page page, string param = null!)
    {
        bool userOptedOut = GetUserOptOutPreference();
        
        string gtmHead = userOptedOut ? "" : Config.GetValue<string>("GTM:Head") ?? "";
        string gtmBody = userOptedOut ? "" : Config.GetValue<string>("GTM:Body") ?? "";

        return new PageViewModel()
        {
            Page = page,
            Param = param,
            GTMHead = gtmHead,
            GTMBody = gtmBody,
        };
    }

    /// <summary>
    /// Returns either the entire slug for the URL (if not null/empty), or "/"
    /// </summary>
    /// <param name="route">Route slug from route binding</param>
    private string GetSlug(string? route) => (string.IsNullOrEmpty(route) ? Request.Path.Value : route) ?? "/";
    
    
    private bool GetUserOptOutPreference()
    {
        string optOutValue = Request.Cookies["PlanTech-CookieAccepted"];

        return optOutValue == "rejected";
    }
}