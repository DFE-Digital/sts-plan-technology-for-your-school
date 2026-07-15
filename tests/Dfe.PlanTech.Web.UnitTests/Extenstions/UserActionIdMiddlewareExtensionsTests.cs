using Dfe.PlanTech.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Extensions;

public class UserActionIdMiddlewareExtensionsTests
{
    [Fact]
    public void UseCorrelationId_WhenCalled_ThenRegistersMiddleware()
    {
        // Arrange
        var appBuilder = Substitute.For<IApplicationBuilder>();

        appBuilder
            .Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>())
            .Returns(appBuilder);

        // Act
        var result = appBuilder.UseUserActionId();

        // Assert
        appBuilder.Received(1).Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
        Assert.Same(appBuilder, result);
    }
}
