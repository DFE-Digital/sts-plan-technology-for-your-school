using System.Security.Claims;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class HomeLinkViewComponentTests
{
    [Fact]
    public void HomeLinkViewComponentReturnsAuthenticatedLinkWhenUserIsAuthenticated()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity("Cookies"))
        };

        var viewComponent = new HomeLinkViewComponent()
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext()
                {
                    HttpContext = httpContext
                }
            }
        };

        var result = viewComponent.Invoke() as ViewViewComponentResult;

        Assert.NotNull(result);
        Assert.Equal("Default", result.ViewName);

        var model = result.ViewData?.Model;
        Assert.NotNull(model);

        var href = model as string;

        Assert.NotNull(href);
        Assert.Equal("/self-assessment", model);
    }

    [Fact]
    public void HomeLinkViewComponentReturnsAuthenticatedLinkWhenUserIsNotuthenticated()
    {
        var httpContext = new DefaultHttpContext
        {
        };

        var viewComponent = new HomeLinkViewComponent()
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext()
                {
                    HttpContext = httpContext
                }
            }
        };

        var result = viewComponent.Invoke() as ViewViewComponentResult;

        Assert.NotNull(result);
        Assert.Equal("Default", result.ViewName);

        var model = result.ViewData?.Model;
        Assert.NotNull(model);

        var href = model as string;

        Assert.NotNull(href);
        Assert.Equal("/", model);
    }
}