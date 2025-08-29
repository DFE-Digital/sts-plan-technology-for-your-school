using Dfe.PlanTech.Web.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Attributes;

public class LogInvalidModelStateAttributeTests
{
    private static ActionExecutingContext BuildContext(
        ILogger<LogInvalidModelStateAttribute>? logger,
        bool isModelStateValid,
        string displayName = "TestController.TestAction")
    {
        var services = new ServiceCollection();
        if (logger is not null)
        {
            services.AddSingleton(logger);
        }

        var http = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        var modelState = new ModelStateDictionary();
        if (!isModelStateValid)
        {
            modelState.AddModelError("Field", "Required");
        }

        var actionDescriptor = new ActionDescriptor { DisplayName = displayName };
        var actionContext = new ActionContext(http, new RouteData(), actionDescriptor, modelState);

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null);
    }

    [Fact]
    public void OnActionExecuting_Throws_When_Logger_Not_Registered()
    {
        var attribute = new LogInvalidModelStateAttribute();
        var ctx = BuildContext(logger: null, isModelStateValid: false);

        Assert.Throws<ArgumentNullException>(() => attribute.OnActionExecuting(ctx));
    }

    [Fact]
    public void OnActionExecuting_DoesNotLog_When_ModelState_Valid()
    {
        var logger = Substitute.For<ILogger<LogInvalidModelStateAttribute>>();
        var attribute = new LogInvalidModelStateAttribute();
        var ctx = BuildContext(logger, isModelStateValid: true);

        attribute.OnActionExecuting(ctx);

        logger.DidNotReceiveWithAnyArgs().Log(
            default, default, default!, default, default!);
    }

    [Fact]
    public void OnActionExecuting_LogsError_When_ModelState_Invalid()
    {
        var logger = Substitute.For<ILogger<LogInvalidModelStateAttribute>>();
        var attribute = new LogInvalidModelStateAttribute();
        var displayName = "SampleController.SampleAction";
        var ctx = BuildContext(logger, isModelStateValid: false, displayName: displayName);

        attribute.OnActionExecuting(ctx);

        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o =>
                o.ToString()!.Contains("Not able to validate model state") &&
                o.ToString()!.Contains(displayName)),
            Arg.Is<Exception?>(e => e == null),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
