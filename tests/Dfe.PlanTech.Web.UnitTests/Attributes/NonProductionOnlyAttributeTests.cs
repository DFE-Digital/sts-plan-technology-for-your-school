using Dfe.PlanTech.Web.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Attributes;

public class NonProductionOnlyAttributeTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Test")]
    public void OnActionExecuting_AllowsNonProduction(string environmentName)
    {
        var attribute = new NonProductionOnlyAttribute();
        var context = CreateContext(environmentName);

        attribute.OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Uat")]
    public void OnActionExecuting_ReturnsNotFound_ForOtherEnvironments(string environmentName)
    {
        var attribute = new NonProductionOnlyAttribute();
        var context = CreateContext(environmentName);

        attribute.OnActionExecuting(context);

        Assert.IsType<NotFoundResult>(context.Result);
    }

    private static ActionExecutingContext CreateContext(string environmentName)
    {
        var services = new ServiceCollection();

        var env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns(environmentName);

        services.AddSingleton(env);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null);
    }
}
