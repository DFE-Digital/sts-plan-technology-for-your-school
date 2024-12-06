using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Route("/cookies")]
public class CookiesController(ILogger<CookiesController> logger, ICookieService cookieService) : BaseController<CookiesController>(logger)
{
    private const string CookiesSlug = "cookies";
    private readonly ICookieService _cookieService = cookieService;

    [HttpGet]
    public async Task<IActionResult> GetCookiesPage([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
    {
        var cookiesPageContent = await getPageQuery.GetPageBySlug(CookiesSlug, cancellationToken);

        var referrerUrl = HttpContext.Request.Headers.Referer.ToString();

        CookiesViewModel cookiesViewModel = new()
        {
            Title = cookiesPageContent?.Title ?? new Title() { Text = "Cookies" },
            Content = cookiesPageContent?.Content ?? [],
            Cookie = _cookieService.Cookie,
            ReferrerUrl = referrerUrl ?? "",
        };

        return View("Cookies", cookiesViewModel);
    }

    [HttpPost]
    public IActionResult SetCookiePreference(string userPreference, bool isCookiesPage = false)
    {
        if (!bool.TryParse(userPreference, out bool userAcceptsCookies))
        {
            throw new ArgumentException("Can't convert preference", userPreference);
        }
        _cookieService.SetCookieAcceptance(userAcceptsCookies);

        if (!isCookiesPage)
        {
            return RedirectToPlaceOfOrigin();
        }

        TempData[CookieConstants.UserPreferenceRecordedKey] = true;

        return RedirectToAction("GetByRoute", "Pages", new { route = CookiesSlug });
    }

    [HttpPost("hidebanner")]
    public IActionResult HideBanner()
    {
        _cookieService.SetVisibility(false);
        return RedirectToPlaceOfOrigin();
    }

    private RedirectResult RedirectToPlaceOfOrigin()
    {
        var returnUrl = Request.Headers.Referer.ToString();

        return Redirect(returnUrl);
    }
}
