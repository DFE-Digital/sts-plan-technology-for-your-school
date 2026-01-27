using System.Security.Claims;
using Dfe.PlanTech.Infrastructure.SignIn.Models;
using Dfe.PlanTech.Web.Authorisation.Handlers;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation.Handlers;

public class UserOrganisationAuthorisationHandlerTests
{
    private static AuthorizationHandlerContext Ctx(
        UserOrganisationAuthorisationRequirement req,
        object resource
    )
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // identity itself is irrelevant here
        return new AuthorizationHandlerContext(new[] { req }, user, resource);
    }

    private static UserOrganisationAuthorisationHandler SUT(
        out ILogger<UserOrganisationAuthorisationHandler> logger
    )
    {
        logger = Substitute.For<ILogger<UserOrganisationAuthorisationHandler>>();
        return new UserOrganisationAuthorisationHandler(logger);
    }

    private static UserAuthorisationResult BuildUserAuthorisationResult(
        bool pageRequiresAuthorisation,
        bool isAuthorised,
        bool isAuthenticated
    )
    {
        var userAuthorisationStatus = new UserAuthorisationStatus(isAuthenticated, isAuthorised);
        return new UserAuthorisationResult(pageRequiresAuthorisation, userAuthorisationStatus);
    }

    [Fact]
    public async Task When_Resource_Is_Not_HttpContext_Logs_Error_And_Does_Not_Succeed_Or_Fail()
    {
        var handler = SUT(out var logger);
        var req = new UserOrganisationAuthorisationRequirement();
        var context = Ctx(req, resource: new object());

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
        logger.ReceivedWithAnyArgs(1).Log(default, default, default!, "{Message}", default!);
    }

    [Fact]
    public async Task When_No_UserAuthorisationResult_In_Items_Succeeds()
    {
        var handler = SUT(out _);
        var req = new UserOrganisationAuthorisationRequirement();
        var http = new DefaultHttpContext(); // no Items[UserAuthorisationResult.HttpContextKey]

        var context = Ctx(req, http);
        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task When_Page_Does_Not_Require_Authorisation_Succeeds()
    {
        var handler = SUT(out _);
        var req = new UserOrganisationAuthorisationRequirement();
        var http = new DefaultHttpContext();
        http.Items[UserAuthorisationResult.HttpContextKey] = BuildUserAuthorisationResult(
            false,
            true,
            false
        );

        var context = Ctx(req, http);
        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task When_Page_Requires_Auth_And_User_Can_View_Succeeds()
    {
        var handler = SUT(out _);
        var req = new UserOrganisationAuthorisationRequirement();
        var http = new DefaultHttpContext();
        http.Items[UserAuthorisationResult.HttpContextKey] = BuildUserAuthorisationResult(
            true,
            true,
            false
        );

        var context = Ctx(req, http);
        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task When_SignOut_Url_Succeeds_Even_If_Page_Requires_Auth_And_User_Cannot_View()
    {
        var handler = SUT(out _);
        var req = new UserOrganisationAuthorisationRequirement();
        var http = new DefaultHttpContext();
        http.Request.Path = "/auth/sign-out";
        http.Items[UserAuthorisationResult.HttpContextKey] = BuildUserAuthorisationResult(
            true,
            false,
            false
        );

        var context = Ctx(req, http);
        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task When_Page_Requires_Auth_And_User_Cannot_View_And_Not_SignOut_Fails_With_Handler_Reason()
    {
        var handler = SUT(out _);
        var req = new UserOrganisationAuthorisationRequirement();
        var http = new DefaultHttpContext();
        http.Request.Path = "/some/other";
        http.Items[UserAuthorisationResult.HttpContextKey] = BuildUserAuthorisationResult(
            true,
            false,
            false
        );

        var context = Ctx(req, http);
        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);

        // Ensure the failure reason came from this handler instance
        Assert.Contains(
            context.FailureReasons,
            r => ReferenceEquals(r.Handler, handler) && r.Message.Contains("Missing organisation")
        );
    }
}
