using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Web.Authorisation.Handlers;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class UserAuthorisationMiddlewareResultHandlerTests
{
    private readonly UserAuthorisationMiddlewareResultHandler _handler;
    private readonly RequestDelegate _next;
    private readonly HttpContext _context;
    private readonly AuthorizationPolicy _policy;
    private readonly IAuthenticationService _authenticationService;

    public UserAuthorisationMiddlewareResultHandlerTests()
    {
        _handler = new UserAuthorisationMiddlewareResultHandler();
        _next = Substitute.For<RequestDelegate>();
        _policy = new AuthorizationPolicy([new UserOrganisationAuthorisationRequirement()], []);

        _context = Substitute.For<HttpContext>();
        _context.Response.Returns(Substitute.For<HttpResponse>());
        var serviceProvider = Substitute.For<IServiceProvider>();

        _authenticationService = Substitute.For<IAuthenticationService>();

        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(_authenticationService);
        _context.RequestServices.Returns(serviceProvider);
    }

    [Fact]
    public async Task HandleAsync_WhenForbiddenAndUserMissingOrganisation_ShouldRedirectToOrgErrorPage()
    {
        var authorizationFailure = AuthorizationFailure.Failed([new AuthorizationFailureReason(new UserOrganisationAuthorisationHandler(new NullLogger<UserOrganisationAuthorisationHandler>()), "User missing org")]);
        var authoriseResult = PolicyAuthorizationResult.Forbid(authorizationFailure);

        await _handler.HandleAsync(_next, _context, _policy, authoriseResult);

        _context.Response.Received(1).Redirect(UrlConstants.OrgErrorPage);
    }

    [Fact]
    public async Task HandleAsync_WhenForbiddenButUserHasOrganisation_ShouldNotRedirect()
    {
        var authorizationFailure = AuthorizationFailure.Failed([new AuthorizationFailureReason(new PageModelAuthorisationPolicy(new NullLogger<PageModelAuthorisationPolicy>()), "")]);

        var authoriseResult = PolicyAuthorizationResult.Forbid(authorizationFailure);

        await _handler.HandleAsync(_next, _context, _policy, authoriseResult);

        _context.Response.Received(0).Redirect(UrlConstants.OrgErrorPage);
    }

    [Fact]
    public async Task HandleAsync_WhenNotForbidden_ShouldNotRedirect()
    {
        var authoriseResult = PolicyAuthorizationResult.Success();

        await _handler.HandleAsync(_next, _context, _policy, authoriseResult);

        _context.Response.Received(0).Redirect(UrlConstants.OrgErrorPage);
    }

    [Fact]
    public void GetRedirectUrl_WhenAuthorisationFailureIsNull_ShouldReturnNull()
    {
        var result = UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(null);

        Assert.Null(result);
    }

    [Fact]
    public void GetRedirectUrl_WhenUserMissingOrganisation_ShouldReturnOrgErrorPage()
    {
        var authorizationFailure = AuthorizationFailure.Failed([new AuthorizationFailureReason(new UserOrganisationAuthorisationHandler(new NullLogger<UserOrganisationAuthorisationHandler>()), "")]);

        var result = UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(authorizationFailure);

        Assert.Equal(UrlConstants.OrgErrorPage, result);
    }

    [Fact]
    public void GetRedirectUrl_WhenUserHasOrganisation_ShouldReturnNull()
    {
        var authorizationFailure = AuthorizationFailure.Failed([new AuthorizationFailureReason(new PageModelAuthorisationPolicy(new NullLogger<PageModelAuthorisationPolicy>()), "some other reason")]);

        var result = UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(authorizationFailure);

        Assert.Null(result);
    }
}
