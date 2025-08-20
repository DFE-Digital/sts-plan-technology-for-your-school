using System.Security.Claims;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public void SignOut_Should_Return_SignOut_Result()
        {
            var controller = new AuthController();
            var claims = new List<Claim>();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            controller.HttpContext.User = new ClaimsPrincipal();
            controller.HttpContext.User.AddIdentity(new ClaimsIdentity(claims, OpenIdConnectDefaults.AuthenticationScheme));
            controller.HttpContext.User.AddIdentity(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            var result = controller.SignOut();

            Assert.NotNull(result);
            Assert.IsType<SignOutResult>(result);

            var signOutResult = result as SignOutResult;
            Assert.NotNull(signOutResult);
            Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult!.AuthenticationSchemes);
            Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        }
    }
}
