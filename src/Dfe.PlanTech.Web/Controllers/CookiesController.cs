using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Route("/cookies")]
public class CookiesController(
    ILoggerFactory logger,
    ContentfulService contentfulService,
    CookieService cookieService
) : BaseController<CookiesController>(logger)
{
    private const string CookiesSlug = "cookies";

    private readonly ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    private readonly CookieService _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));

    [HttpGet]
    public async Task<IActionResult> GetCookiesPage()
    {
        var cookiesPageContent = await _contentfulService.GetPageBySlugAsync(CookiesSlug);

        var referrerUrl = HttpContext.Request.Headers.Referer.ToString();

        CookiesViewModel cookiesViewModel = new()
        {
            Title = cookiesPageContent?.Title ?? new ComponentTitleEntry("Cookies"),
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
