using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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

        ControllerContext = new ControllerContext { HttpContext = httpContext };

        TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
    }
}
