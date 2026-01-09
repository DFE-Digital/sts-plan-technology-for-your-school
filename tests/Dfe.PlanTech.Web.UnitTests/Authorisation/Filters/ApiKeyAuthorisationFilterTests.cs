using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.UnitTests.Shared.Extensions;
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

        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, []);
    }

    private static ApiAuthenticationConfiguration ConfigWithKey(string key) =>
        new ApiAuthenticationConfiguration { KeyValue = key };

    [Fact]
    public void OnAuthorization_When_ConfigMissingApiKey_Returns_Unauthorized_And_LogsError()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ApiKeyAuthorisationFilter>>();
        var cfg = ConfigWithKey(null!); // HasApiKey == false
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
        logger
            .ReceivedWithAnyArgs()
            .Log(LogLevel.Information, default, default!, default, default!);
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
        logger
            .ReceivedWithAnyArgs()
            .Log(LogLevel.Information, default, default!, default, default!);
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

    private const string KeyValue = "mock-refresh-api-key";
    private readonly ApiKeyAuthorisationFilter _authorisationFilter;
    private readonly ILogger<ApiKeyAuthorisationFilter> _logger = Substitute.For<
        ILogger<ApiKeyAuthorisationFilter>
    >();

    public ApiKeyAuthorisationFilterTests()
    {
        var config = new ApiAuthenticationConfiguration { KeyValue = KeyValue };
        _authorisationFilter = new ApiKeyAuthorisationFilter(_logger, config);
    }

    [Fact]
    public void ShouldReturn_Unauthorised_Result_If_Missing_ApiKey()
    {
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
        };
        var filterContext = new AuthorizationFilterContext(actionContext, []);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }

    [Theory]
    [InlineData("invalid-api-key")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Bearer invalid-api-key")]
    public void ShouldReturn_Unauthorised_Result_If_Invalid_ApiKey(string? apiKey)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(ApiKeyAuthorisationFilter.AuthHeaderKey, apiKey);
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
        };
        var filterContext = new AuthorizationFilterContext(actionContext, []);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }

    [Fact]
    public void Should_Continue_Authorised_If_Valid_ApiKey()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(
            ApiKeyAuthorisationFilter.AuthHeaderKey,
            ApiKeyAuthorisationFilter.AuthValuePrefix + KeyValue
        );
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
        };
        var filterContext = new AuthorizationFilterContext(actionContext, []);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsNotType<UnauthorizedResult>(filterContext.Result);
        Assert.Null(filterContext.Result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Handle_MissingApiKeyConfiguration(string? apiKey)
    {
        var config = new ApiAuthenticationConfiguration { KeyValue = apiKey! };
        var authorisationFilter = new ApiKeyAuthorisationFilter(_logger, config);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(
            ApiKeyAuthorisationFilter.AuthHeaderKey,
            ApiKeyAuthorisationFilter.AuthValuePrefix + KeyValue
        );
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
        };
        var filterContext = new AuthorizationFilterContext(actionContext, []);
        authorisationFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);

        var loggedMessages = _logger.ReceivedLogMessages().ToArray();

        Assert.Single(loggedMessages);
        Assert.Equal($"API key {nameof(config.KeyValue)} is missing", loggedMessages[0].Message);
        Assert.Equal(LogLevel.Error, loggedMessages[0].LogLevel);
    }
}
