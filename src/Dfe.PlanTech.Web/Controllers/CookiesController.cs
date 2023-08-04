using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers
{
    [Route("/cookies")]
    public class CookiesController : Controller
    {
        [HttpPost("accept")]
        public IActionResult Accept()
        {
            CreateCookie("PlanTech-CookieAccepted", "Accepted");
            var host = HttpContext.Request.Host.Value;
            //return Redirect(host);
            return RedirectToAction("/", "Pages");
        }

        [HttpPost("hidebanner")]
        public IActionResult HideBanner()
        {
            CreateCookie("PlanTech-HideCookieBanner", "Hidden");
            var host = HttpContext.Request.Host.Value;
            return Redirect(host);
        }

        private void CreateCookie(string key, string value)
        {
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddYears(1));
            HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }
    }
}
