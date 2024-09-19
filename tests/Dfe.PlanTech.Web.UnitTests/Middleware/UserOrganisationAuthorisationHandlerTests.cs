using System.Security.Claims;
using Dfe.PlanTech;
using Dfe.PlanTech.Infrastructure.SignIns.Models;
using Dfe.PlanTech.Web.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public class UserOrganisationAuthorisationHandlerTests
{
    private readonly UserOrganisationAuthorisationHandlerMock _handler;
    private readonly ILogger<UserOrganisationAuthorisationHandler> _logger;

    public UserOrganisationAuthorisationHandlerTests()
    {
        _logger = Substitute.For<ILogger<UserOrganisationAuthorisationHandlerMock>>();
        _handler = new UserOrganisationAuthorisationHandlerMock(_logger);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenUserCanViewPage()
    {
        var context = new DefaultHttpContext();
        var requirement = new UserOrganisationAuthorisationRequirement();
        context.Items[UserAuthorisationResult.HttpContextKey] = new UserAuthorisationResult(true, new UserAuthorisationStatus(true, true));

        var authContext = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(),
            context
        );

        await _handler.PublicHandleRequirementAsync(authContext, requirement);

        Assert.True(authContext.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenPageRequiresNoAuthorisation()
    {
        var context = new DefaultHttpContext();
        var requirement = new UserOrganisationAuthorisationRequirement();
        context.Items[UserAuthorisationResult.HttpContextKey] = new UserAuthorisationResult(false, new UserAuthorisationStatus(true, false));

        var authContext = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(),
            context
        );

        await _handler.PublicHandleRequirementAsync(authContext, requirement);

        Assert.True(authContext.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldFail_WhenUserIsMissingOrganisation()
    {
        var context = new DefaultHttpContext();
        var requirement = new UserOrganisationAuthorisationRequirement();
        context.Items[UserAuthorisationResult.HttpContextKey] = new UserAuthorisationResult(true, new UserAuthorisationStatus(true, false));

        var authContext = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(),
            context
        );

        await _handler.PublicHandleRequirementAsync(authContext, requirement);

        Assert.False(authContext.HasSucceeded);
        Assert.Single(authContext.FailureReasons);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenRequestIsSignoutUrl()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/auth/sign-out";
        var requirement = new UserOrganisationAuthorisationRequirement();

        var authContext = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(),
            context
        );

        await _handler.PublicHandleRequirementAsync(authContext, requirement);

        Assert.True(authContext.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldLogError_WhenResourceIsNotHttpContext()
    {
        var requirement = new UserOrganisationAuthorisationRequirement();
        var authContext = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(),
            new object()
        );

        await _handler.PublicHandleRequirementAsync(authContext, requirement);

        var matchingCalls = _logger.GetMatchingReceivedMessages($"Expected resource to be HttpContext but received {typeof(object)}", LogLevel.Error);
        Assert.Single(matchingCalls);
    }
}

public class UserOrganisationAuthorisationHandlerMock(ILogger<UserOrganisationAuthorisationHandler> logger) : UserOrganisationAuthorisationHandler(logger)
{
    public Task PublicHandleRequirementAsync(AuthorizationHandlerContext context, UserOrganisationAuthorisationRequirement requirement)
    {
        return HandleRequirementAsync(context, requirement);
    }
}
