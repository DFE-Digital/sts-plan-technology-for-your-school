using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class LogInvalidModelStateTests
{
    [Fact]
    public void Should_Call_Logger_When_Invalid_Model_State()
    {
        var logger = Substitute.For<ILogger<LogInvalidModelStateAttribute>>();
        var filters = new List<IFilterMetadata>();
        var actionArguments = new Dictionary<string, object?>();
        var controller = Substitute.For<Controller>();

        var context = Substitute.For<HttpContext>();
        context.RequestServices.GetService(typeof(ILogger<LogInvalidModelStateAttribute>)).Returns(logger);

        var actionContext = new ActionContext
        {
            HttpContext = context,
            RouteData = Substitute.For<RouteData>(),
            ActionDescriptor = Substitute.For<ActionDescriptor>(),
        };

        var actionExecutingContext = new ActionExecutingContext(actionContext, filters, actionArguments, controller);
        actionExecutingContext.ModelState.AddModelError("Property", "Is Missing");

        var filter = new LogInvalidModelStateAttribute();
        filter.OnActionExecuting(actionExecutingContext);

        Assert.Single(logger.ReceivedCalls());
    }
}
