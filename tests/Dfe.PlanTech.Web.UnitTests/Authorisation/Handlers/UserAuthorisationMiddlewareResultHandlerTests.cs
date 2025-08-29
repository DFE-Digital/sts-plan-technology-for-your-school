using System.Text.Encodings.Web;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Authorisation.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation.Handlers;

public class UserAuthorisationMiddlewareResultHandlerTests
{
    private static AuthorizationPolicy AnyPolicy()
        => new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

    private static PolicyAuthorizationResult ForbiddenWith(params AuthorizationFailureReason[] reasons)
        => PolicyAuthorizationResult.Forbid(AuthorizationFailure.Failed(reasons));

    private static AuthorizationFailureReason ReasonFrom(IAuthorizationHandler handler, string message = "")
        => new AuthorizationFailureReason(handler, message);

    private static HttpContext HttpWithAuthServices()
    {
        var services = new ServiceCollection();

        // Either minimal core services:
        services.AddAuthenticationCore();
        services.AddLogging();
        services.AddOptions();

        services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, DummyHandler>("Test", _ => { });

        services.AddAuthorization(); // not strictly required here, but harmless

        return new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
    }

    [Fact]
    public async Task Forbidden_With_UserOrganisationFailure_Redirects_To_OrgErrorPage_And_Skips_Next()
    {
        // Arrange
        var logger = Substitute.For<ILogger<UserOrganisationAuthorisationHandler>>();
        var http = new DefaultHttpContext();
        var calledNext = false;
        RequestDelegate next = _ => { calledNext = true; return Task.CompletedTask; };

        var failure = ReasonFrom(new UserOrganisationAuthorisationHandler(logger), "No org");
        var authorizeResult = ForbiddenWith(failure);
        var policy = AnyPolicy();

        var sut = new UserAuthorisationMiddlewareResultHandler();

        // Act
        await sut.HandleAsync(next, http, policy, authorizeResult);

        // Assert: redirect happened and next not called
        Assert.Equal(StatusCodes.Status302Found, http.Response.StatusCode);
        Assert.Equal(UrlConstants.OrgErrorPage, http.Response.Headers["Location"].ToString());
        Assert.False(calledNext);
    }

    [Fact]
    public async Task Forbidden_With_OtherFailure_DoesNotRedirect_DefaultHandler_Writes_403_And_Skips_Next()
    {
        // Arrange
        var http = HttpWithAuthServices();
        var calledNext = false;
        RequestDelegate next = _ => { calledNext = true; return Task.CompletedTask; };

        // Use an unrelated handler to simulate a different failure reason
        var otherHandler = new DenyAnonymousAuthorizationRequirement(); // just a stand-in
        var failure = ReasonFrom(new StubHandler(), "Other reason");
        var authorizeResult = ForbiddenWith(failure);
        var policy = AnyPolicy();

        var sut = new UserAuthorisationMiddlewareResultHandler();

        // Act
        await sut.HandleAsync(next, http, policy, authorizeResult);

        // Assert: default AuthorizationMiddlewareResultHandler sets 403; no redirect; next not called
        Assert.Equal(StatusCodes.Status403Forbidden, http.Response.StatusCode);
        Assert.False(http.Response.Headers.ContainsKey("Location"));
        Assert.False(calledNext);
    }

    [Fact]
    public async Task Authorized_Success_Calls_Next()
    {
        // Arrange
        var http = new DefaultHttpContext();
        var calledNext = false;
        RequestDelegate next = _ => { calledNext = true; return Task.CompletedTask; };

        var authorizeResult = PolicyAuthorizationResult.Success();
        var policy = AnyPolicy();

        var sut = new UserAuthorisationMiddlewareResultHandler();

        // Act
        await sut.HandleAsync(next, http, policy, authorizeResult);

        // Assert
        Assert.True(calledNext);
        // No redirect/status manipulation expected
        Assert.Equal(StatusCodes.Status200OK, http.Response.StatusCode); // default if nothing wrote
    }

    [Fact]
    public void GetRedirectUrl_NullFailure_ReturnsNull()
    {
        Assert.Null(UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(null));
    }

    [Fact]
    public void GetRedirectUrl_With_OtherFailure_ReturnsNull()
    {
        var fail = AuthorizationFailure.Failed(new[]
        {
            ReasonFrom(new StubHandler(), "not org missing")
        });

        Assert.Null(UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(fail));
    }

    [Fact]
    public void GetRedirectUrl_With_UserOrganisationFailure_Returns_OrgErrorPage()
    {
        var logger = Substitute.For<ILogger<UserOrganisationAuthorisationHandler>>();

        var fail = AuthorizationFailure.Failed(new[]
        {
            ReasonFrom(new UserOrganisationAuthorisationHandler(logger), "org missing")
        });

        Assert.Equal(UrlConstants.OrgErrorPage, UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(fail));
    }

    // Minimal stub for a non-org handler
    private sealed class StubHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context) => Task.CompletedTask;
    }
}

public sealed class DummyHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DummyHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());
}
