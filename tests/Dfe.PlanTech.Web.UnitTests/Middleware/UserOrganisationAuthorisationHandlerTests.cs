using System.Security.Claims;
using Dfe.PlanTech.Infrastructure.SignIn.Models;
using Dfe.PlanTech.Web.Authorisation.Handlers;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Middleware
{
    public class UserOrganisationAuthorisationHandlerTests
    {
        private readonly UserOrganisationAuthorisationHandlerMock _handler;
        private readonly ILogger<UserOrganisationAuthorisationHandler> _logger;

        public UserOrganisationAuthorisationHandlerTests()
        {
            _logger = Substitute.For<ILogger<UserOrganisationAuthorisationHandler>>();
            _handler = new UserOrganisationAuthorisationHandlerMock(_logger);
        }

        [Fact]
        public async Task HandleRequirementAsync_ShouldSucceed_WhenUserCanViewPage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var requirement = new UserOrganisationAuthorisationRequirement();

            context.Items[UserAuthorisationResult.HttpContextKey] =
                    new UserAuthorisationResult(true, new UserAuthorisationStatus(true, true));

            var authContext = new AuthorizationHandlerContext(
                new[] { requirement },
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

            context.Items[UserAuthorisationResult.HttpContextKey] =
                    new UserAuthorisationResult(true, new UserAuthorisationStatus(true, true));

            var authContext = new AuthorizationHandlerContext(
                new[] { requirement },
                new ClaimsPrincipal(),
                context
            );

            await _handler.PublicHandleRequirementAsync(authContext, requirement);

            Assert.True(authContext.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_ShouldFail_WhenUserIsMissingOrganisation()
        {
            var logger = Substitute.For<ILogger<UserOrganisationAuthorisationHandler>>();
            var handler = new UserOrganisationAuthorisationHandlerMock(logger);

            var context = new DefaultHttpContext();
            context.Request.Path = "/not-signout";

            var requirement = new UserOrganisationAuthorisationRequirement();

            context.Items[UserAuthorisationResult.HttpContextKey] =
                new UserAuthorisationResult(
                    true,
                    new UserAuthorisationStatus(true, false)
                );

            var authContext = new AuthorizationHandlerContext(
                new[] { requirement },
                new ClaimsPrincipal(),
                context
            );

            await handler.PublicHandleRequirementAsync(authContext, requirement);

            Assert.False(authContext.HasSucceeded);
            Assert.True(authContext.HasFailed);
            Assert.NotEmpty(authContext.FailureReasons);
        }



        [Fact]
        public async Task HandleRequirementAsync_ShouldSucceed_WhenRequestIsSignoutUrl()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/auth/sign-out";
            var requirement = new UserOrganisationAuthorisationRequirement();

            var authContext = new AuthorizationHandlerContext(
                new[] { requirement },
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
            var notHttpContextResource = new object();

            var authContext = new AuthorizationHandlerContext(
                new[] { requirement },
                new ClaimsPrincipal(),
                notHttpContextResource
            );

            await _handler.PublicHandleRequirementAsync(authContext, requirement);

            Assert.False(authContext.HasSucceeded);
            Assert.False(authContext.HasFailed);

            _logger.Received(1).Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => o != null && o.ToString() == $"Expected resource to be HttpContext but received {typeof(object)}"),
                    Arg.Any<Exception?>(),
                    Arg.Any<Func<object, Exception?, string>>()
                );
        }
    }

    public class UserOrganisationAuthorisationHandlerMock : UserOrganisationAuthorisationHandler
    {
        public UserOrganisationAuthorisationHandlerMock(ILogger<UserOrganisationAuthorisationHandler> logger)
            : base(logger) { }

        public Task PublicHandleRequirementAsync(AuthorizationHandlerContext context, UserOrganisationAuthorisationRequirement requirement)
            => HandleRequirementAsync(context, requirement);
    }
}
