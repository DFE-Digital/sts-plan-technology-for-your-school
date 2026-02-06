using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    public AuthController() { }

    [Authorize]
    [HttpGet("sign-out")]
    public new IActionResult SignOut() =>
        new SignOutResult([
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme,
        ]);
}
