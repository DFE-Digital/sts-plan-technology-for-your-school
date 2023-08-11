using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
    public async Task<IActionResult> GetByRoute(string route, [FromServices] GetPageQuery query, CancellationToken cancellationToken)
    {
        string slug = GetSlug(route);
        string param = "";

        if (TempData[slug] != null && TempData[slug] is string) param = TempData[slug] as string ?? "";

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

    private PageViewModel CreatePageModel(Page page, string param = null!)
    {
        bool acceptCookies = GetUserOptOutPreference();

        string gtmHead = acceptCookies ? Config.GetValue<string>("GTM:Head") ?? "" : "";
        string gtmBody = acceptCookies ? Config.GetValue<string>("GTM:Body") ?? "" : "";

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
        var userPreferenceCookieValue = Request.Cookies["cookies_preferences_set"];

        if (bool.TryParse(userPreferenceCookieValue, out bool acceptCookies))
        {
            return acceptCookies;
        }

        return false;
    }
}