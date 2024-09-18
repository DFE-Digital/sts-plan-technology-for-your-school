using Dfe.PlanTech.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.Authorisation.Tests
{
    public class UserAuthorisationMiddlewareResultHandlerTests
    {
        private readonly UserAuthorisationMiddlewareResultHandler _handler;
        private readonly RequestDelegate _next;
        private readonly HttpContext _context;
        private readonly AuthorizationPolicy _policy;

        public UserAuthorisationMiddlewareResultHandlerTests()
        {
            _handler = new UserAuthorisationMiddlewareResultHandler();
            _next = Substitute.For<RequestDelegate>();
            _context = Substitute.For<HttpContext>();
            _policy = new AuthorizationPolicy([new UserOrganisationAuthorisationRequirement()], []);
        }

        [Fact]
        public async Task HandleAsync_WhenForbiddenAndUserMissingOrganisation_ShouldRedirectToOrgErrorPage()
        {
            var failures = new List<AuthorizationFailureReason>();
            var authorizationFailure = AuthorizationFailure.Failed(failures);
            var authoriseResult = PolicyAuthorizationResult.Forbid(authorizationFailure);
            _context.Response.Returns(Substitute.For<HttpResponse>());

            await _handler.HandleAsync(_next, _context, _policy, authoriseResult);

            _context.Response.Received(1).Redirect(UrlConstants.OrgErrorPage);
        }

        [Fact]
        public async Task HandleAsync_WhenForbiddenButUserHasOrganisation_ShouldNotRedirect()
        {
            var authorizationFailure = AuthorizationFailure.Failed([new AuthorizationFailureReason(new PageModelAuthorisationPolicy(new NullLogger<PageModelAuthorisationPolicy>()), "")]);

            var authoriseResult = PolicyAuthorizationResult.Forbid(authorizationFailure);
            _context.Response.Returns(Substitute.For<HttpResponse>());

            await _handler.HandleAsync(_next, _context, _policy, authoriseResult);

            await _next.Received(1).Invoke(_context);
        }

        [Fact]
        public async Task HandleAsync_WhenNotForbidden_ShouldCallDefaultHandler()
        {
            var authoriseResult = PolicyAuthorizationResult.Success();
            _context.Response.Returns(Substitute.For<HttpResponse>());

            await _handler.HandleAsync(_next, _context, _policy, authoriseResult);

            await _next.Received(1).Invoke(_context);
        }

        [Fact]
        public void GetRedirectUrl_WhenAuthorisationFailureIsNull_ShouldReturnNull()
        {
            var result = UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(null, _context);

            Assert.Null(result);
        }

        [Fact]
        public void GetRedirectUrl_WhenUserMissingOrganisation_ShouldReturnOrgErrorPage()
        {
            var authorizationFailure = AuthorizationFailure.Failed([new AuthorizationFailureReason(new UserOrganisationAuthorisationHandler(new NullLogger<UserOrganisationAuthorisationHandler>()), "")]);

            var result = UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(authorizationFailure, _context);

            Assert.Equal(UrlConstants.OrgErrorPage, result);
        }

        [Fact]
        public void GetRedirectUrl_WhenUserHasOrganisation_ShouldReturnNull()
        {
            var authorizationFailure = AuthorizationFailure.Failed([new AuthorizationFailureReason(new PageModelAuthorisationPolicy(new NullLogger<PageModelAuthorisationPolicy>()), "some other reason")]);

            var result = UserAuthorisationMiddlewareResultHandler.GetRedirectUrl(authorizationFailure, _context);

            Assert.Null(result);
        }
    }
}
