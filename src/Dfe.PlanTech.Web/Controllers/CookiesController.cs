using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;

namespace Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Authorize]
[Route("/cookies")]
public class CookiesController : BaseController<CookiesController>
{

    public CookiesController(ILogger<CookiesController> logger) : base(logger) { }

    public async Task<IActionResult> GetCookiesPage([FromServices] GetPageQuery getPageQuery)
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
        HttpContext httpContext = HttpContext;

        var cookieOptions = new CookieOptions
        {
            Expires = DateTime.Now.AddYears(1)
        };
        
        httpContext.Response.Cookies.Append("acceptAnalyticCookiesPreference", userPreference, cookieOptions);
        
        TempData["UserPreferenceRecorded"] = true;
        
        return RedirectToAction("GetByRoute", "Pages", new { route = "cookies" });
    }
    
    [HttpPost("accept")]
    public IActionResult Accept()
    {
        CreateCookie("PlanTech-CookieAccepted", "Accepted");
        return RedirectToPlaceOfOrigin();
    }

    [HttpPost("hidebanner")]
    public IActionResult HideBanner()
    {
        CreateCookie("PlanTech-HideCookieBanner", "Hidden");
        return RedirectToPlaceOfOrigin();
    }

    private IActionResult RedirectToPlaceOfOrigin()
    {
        var returnUrl = Request.Headers["Referer"].ToString();
        return Redirect(returnUrl);
    }

    private void CreateCookie(string key, string value)
    {
        CookieOptions cookieOptions = new CookieOptions();
        cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddYears(1));
        HttpContext.Response.Cookies.Append(key, value, cookieOptions);
    }
    
}

