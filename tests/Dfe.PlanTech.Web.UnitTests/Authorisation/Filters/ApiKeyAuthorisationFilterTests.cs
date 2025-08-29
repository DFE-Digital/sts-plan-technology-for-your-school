using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Web.Authorisation.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation.Filters;

public class ApiKeyAuthorisationFilterTests
{
    private static AuthorizationFilterContext BuildContext(string? authHeaderValue = null)
    {
        var http = new DefaultHttpContext();
        if (authHeaderValue != null)
        {
            http.Request.Headers[ApiKeyAuthorisationFilter.AuthHeaderKey] = authHeaderValue;
        }

        var actionContext = new ActionContext(
            http,
            new RouteData(),
            new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    private static ApiAuthenticationConfiguration ConfigWithKey(string? key)
        => new ApiAuthenticationConfiguration { KeyValue = key };

    [Fact]
    public void OnAuthorization_When_ConfigMissingApiKey_Returns_Unauthorized_And_LogsError()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ApiKeyAuthorisationFilter>>();
        var cfg = ConfigWithKey(null); // HasApiKey == false
        var sut = new ApiKeyAuthorisationFilter(logger, cfg);
        var ctx = BuildContext("Bearer whatever");

        // Act
        sut.OnAuthorization(ctx);

        // Assert
        Assert.IsType<UnauthorizedResult>(ctx.Result);
        logger.ReceivedWithAnyArgs().Log(LogLevel.Error, default, default!, default, default!);
    }

    [Fact]
    public void OnAuthorization_When_HeaderMissing_Returns_Unauthorized_And_LogsInfo()
    {
        var logger = Substitute.For<ILogger<ApiKeyAuthorisationFilter>>();
        var cfg = ConfigWithKey("secret");
        var sut = new ApiKeyAuthorisationFilter(logger, cfg);
        var ctx = BuildContext(authHeaderValue: null);

        sut.OnAuthorization(ctx);

        Assert.IsType<UnauthorizedResult>(ctx.Result);
        logger.ReceivedWithAnyArgs().Log(LogLevel.Information, default, default!, default, default!);
    }

    [Fact]
    public void OnAuthorization_When_HeaderWithoutBearerPrefix_Returns_Unauthorized()
    {
        var logger = Substitute.For<ILogger<ApiKeyAuthorisationFilter>>();
        var cfg = ConfigWithKey("secret");
        var sut = new ApiKeyAuthorisationFilter(logger, cfg);
        var ctx = BuildContext("Token secret"); // wrong prefix

        sut.OnAuthorization(ctx);

        Assert.IsType<UnauthorizedResult>(ctx.Result);
    }

    [Fact]
    public void OnAuthorization_When_HeaderBearerButEmptyToken_Returns_Unauthorized()
    {
        var logger = Substitute.For<ILogger<ApiKeyAuthorisationFilter>>();
        var cfg = ConfigWithKey("secret");
        var sut = new ApiKeyAuthorisationFilter(logger, cfg);
        var ctx = BuildContext("Bearer "); // empty token

        sut.OnAuthorization(ctx);

        Assert.IsType<UnauthorizedResult>(ctx.Result);
        logger.ReceivedWithAnyArgs().Log(LogLevel.Information, default, default!, default, default!);
    }

    [Fact]
    public void OnAuthorization_When_TokenDoesNotMatch_Returns_Unauthorized()
    {
        var logger = Substitute.For<ILogger<ApiKeyAuthorisationFilter>>();
        var cfg = ConfigWithKey("secret");
        var sut = new ApiKeyAuthorisationFilter(logger, cfg);
        var ctx = BuildContext("Bearer wrong");

        sut.OnAuthorization(ctx);

        Assert.IsType<UnauthorizedResult>(ctx.Result);
    }

    [Fact]
    public void OnAuthorization_When_TokenMatches_Allows_Request()
    {
        var logger = Substitute.For<ILogger<ApiKeyAuthorisationFilter>>();
        var cfg = ConfigWithKey("secret");
        var sut = new ApiKeyAuthorisationFilter(logger, cfg);
        var ctx = BuildContext("Bearer secret");

        sut.OnAuthorization(ctx);

        // No result set => pipeline continues
        Assert.Null(ctx.Result);
    }
}
