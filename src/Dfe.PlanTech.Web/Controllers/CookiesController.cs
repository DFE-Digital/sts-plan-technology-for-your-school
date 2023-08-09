using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Application.Cookie.Interfaces;
using Dfe.PlanTech.Application.Cookie.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Polly;

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
        _cookieService.SetPreference("true");
        return RedirectToPlaceOfOrigin();
    }

    [HttpPost("reject")]
    public IActionResult Reject()
    {
        _cookieService.SetPreference("false");
        return RedirectToPlaceOfOrigin();
    }

    [HttpPost("hidebanner")]
    public IActionResult HideBanner()
    {
        // _cookieService.SetPreference("false");
        return RedirectToPlaceOfOrigin();
    }

    private IActionResult RedirectToPlaceOfOrigin()
    {
        var returnUrl = Request.Headers["Referer"].ToString();
        return Redirect(returnUrl);
    }



    public IActionResult GetCookiesPage([FromServices] GetPageQuery getPageQuery)
    {
        //TODO: Need to setup content in contentful.
        //Page cookiesPageContent = await getPageQuery.GetPageBySlug("cookies", CancellationToken.None);

        CookiesViewModel cookiesViewModel = new CookiesViewModel()
        {
            //TODO: Uncomment when the data is setup in contentful.
            // Title = cookiesPageContent.Title ?? throw new NullReferenceException(nameof(cookiesPageContent.Title)),
            // Content = cookiesPageContent.Content,
            Title = new Title()
            {
                Text = "Cookies"
            }
        };


        return View("Cookies", cookiesViewModel);
    }

    [HttpPost]
    public IActionResult CookiePreference(string userPreference)
    {
        _cookieService.SetPreference(userPreference);
        TempData["UserPreferenceRecorded"] = true;
        return RedirectToAction("GetByRoute", "Pages", new { route = "cookies" });
    }
}