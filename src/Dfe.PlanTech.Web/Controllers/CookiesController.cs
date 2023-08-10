using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Cookie.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/cookies")]
public class CookiesController : BaseController<CookiesController>
{
    private ICookieService _cookieService;

    public CookiesController(ILogger<CookiesController> logger, ICookieService cookieService) : base(logger)
    {
        _cookieService = cookieService;
    }

    [HttpPost("accept")]
    public IActionResult Accept()
    {
        _cookieService.SetPreference(true);
        return RedirectToPlaceOfOrigin();
    }

    [HttpPost("reject")]
    public IActionResult Reject()
    {
        _cookieService.SetPreference(false);
        return RedirectToPlaceOfOrigin();
    }

    [HttpPost("hidebanner")]
    public IActionResult HideBanner()
    {
        _cookieService.SetVisibility(false);
        return RedirectToPlaceOfOrigin();
    }

    private IActionResult RedirectToPlaceOfOrigin()
    {
        var returnUrl = Request.Headers["Referer"].ToString();
        return Redirect(returnUrl);
    }

    public IActionResult GetCookiesPage([FromServices] GetPageQuery getPageQuery)
    {
        Page cookiesPageContent = await getPageQuery.GetPageBySlug("cookies", CancellationToken.None);

        CookiesViewModel cookiesViewModel = new CookiesViewModel()
        {
             Title = cookiesPageContent.Title ?? throw new NullReferenceException(nameof(cookiesPageContent.Title)),
             Content = cookiesPageContent.Content
        };


        return View("Cookies", cookiesViewModel);
    }

    [HttpPost]
    public IActionResult CookiePreference(string userPreference)
    {
        bool.TryParse(userPreference, out bool preference);
        _cookieService.SetPreference(preference);
        TempData["UserPreferenceRecorded"] = true;
        return RedirectToAction("GetByRoute", "Pages", new { route = "cookies" });
    }
}