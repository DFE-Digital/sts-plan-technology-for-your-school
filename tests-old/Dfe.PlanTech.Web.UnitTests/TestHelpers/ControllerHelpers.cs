using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests;

public static class ControllerHelpers
{
    public static ControllerContext SubstituteControllerContext()
    {
        var httpContext = Substitute.For<HttpContext>();
        var session = Substitute.For<ISession>();
        httpContext.Session.Returns(session);
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider
                .GetService(Arg.Is(typeof(ITempDataDictionaryFactory)))
                .Returns(Substitute.For<ITempDataDictionaryFactory>());
        serviceProvider
                .GetService(Arg.Is(typeof(IUrlHelperFactory)))
                .Returns(Substitute.For<IUrlHelperFactory>());
        httpContext.RequestServices.Returns(serviceProvider);


        return new ControllerContext { HttpContext = httpContext };
    }
}
