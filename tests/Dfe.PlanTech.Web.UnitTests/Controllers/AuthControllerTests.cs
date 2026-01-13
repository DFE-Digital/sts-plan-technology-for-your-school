using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Tests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public void SignOut_ReturnsSignOutResult_WithCorrectSchemes()
        {
            var controller = new AuthController();

            var result = controller.SignOut();

            var signOutResult = Assert.IsType<SignOutResult>(result);
            Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
            Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        }
    }
}
