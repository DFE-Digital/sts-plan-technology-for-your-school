using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{

    public AuthController()
    {
        var testing = 5;
        var test = "test";
    }

    [Authorize]
    [HttpGet("sign-out")]
    public new IActionResult SignOut() => new SignOutResult(new[] { OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme });
}