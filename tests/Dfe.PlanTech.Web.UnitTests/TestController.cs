using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests;

public sealed class TestController : Controller
{
    public TestController(string? path = null)
    {
        var httpContext = new DefaultHttpContext();
        if (!string.IsNullOrWhiteSpace(path))
        {
            httpContext.Request.Path = "/test-path";
        }

        ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
        };

        TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
    }
}
