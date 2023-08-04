using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfe.PlanTech.Web.Controllers
{
    [Route("/cookies")]
    public class CookiesController : Controller
    {
        [HttpPost("accept")]
        public IActionResult Accept()
        {
            CreateCookie("cookies_preferences_set", "true");
            return RedirectToPlaceOfOrigin();
        }

        [HttpPost("hidebanner")]
        public IActionResult HideBanner()
        {
            CreateCookie("cookies_preferences_hidden", "true");
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
            cookieOptions.Secure = true;
            cookieOptions.HttpOnly= true;
            cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddYears(1));
            HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }
    }
}
