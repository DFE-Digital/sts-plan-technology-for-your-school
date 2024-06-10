using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Route("/cookies")]
public class CookiesController : BaseController<CookiesController>
{
    private const string CookiesSlug = "cookies";
    private readonly ICookieService _cookieService;

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
        _cookieService.RejectCookies();
        return RedirectToPlaceOfOrigin();
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

    public async Task<IActionResult> GetCookiesPage([FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken)
    {
        var cookiesPageContent = await getPageQuery.GetPageBySlug(CookiesSlug, cancellationToken);

        CookiesViewModel cookiesViewModel = new()
        {
            Title = cookiesPageContent?.Title ?? new Title() { Text = "Cookies" },
            Content = cookiesPageContent?.Content ?? new List<ContentComponent>(0)
        };

        return View("Cookies", cookiesViewModel);
    }

    [HttpPost]
    public IActionResult CookiePreference(string userPreference)
    {
        if (bool.TryParse(userPreference, out bool preference))
        {
            if (preference)
            {
                _cookieService.SetPreference(preference);
            }
            else
            {
                _cookieService.RejectCookies();
            }
            TempData["UserPreferenceRecorded"] = true;
            return RedirectToAction("GetByRoute", "Pages", new { route = "cookies" });
        }
        else
        {
            throw new ArgumentException("Can't convert preference", nameof(userPreference));
        }
    }
}