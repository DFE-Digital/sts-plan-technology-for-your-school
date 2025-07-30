using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class ChangeAnswersControllerTests
    {
        private readonly ChangeAnswersController _controller;
        private readonly IChangeAnswersRouter _changeAnswersRouter;
        private readonly IUserJourneyMissingContentExceptionHandler _exceptionHandler;
        private readonly IUser _user;

        private readonly string _categorySlug = "test-category";
        private readonly string _sectionSlug = "test-section";

        public ChangeAnswersControllerTests()
        {
            var logger = Substitute.For<ILogger<ChangeAnswersController>>();
            _user = Substitute.For<IUser>();
            _controller = new ChangeAnswersController(logger, _user)
            {
                ControllerContext = ControllerHelpers.SubstituteControllerContext()
            };

            _changeAnswersRouter = Substitute.For<IChangeAnswersRouter>();
            _exceptionHandler = Substitute.For<IUserJourneyMissingContentExceptionHandler>();
        }

        [Fact]
        public async Task ChangeAnswersPage_Should_Throw_ArgumentNullException_When_SectionSlug_IsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.ChangeAnswersPage(_categorySlug, null!, _changeAnswersRouter, _exceptionHandler));
        }

        [Fact]
        public async Task ChangeAnswersPage_Should_Throw_ArgumentException_When_SectionSlug_IsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.ChangeAnswersPage(_categorySlug, "", _changeAnswersRouter, _exceptionHandler));
        }

        [Fact]
        public async Task ChangeAnswersPage_Should_Throw_ArgumentNullException_When_CategorySlug_IsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.ChangeAnswersPage(null!, _sectionSlug, _changeAnswersRouter, _exceptionHandler));
        }

        [Fact]
        public async Task ChangeAnswersPage_Should_Throw_ArgumentException_When_CategorySlug_IsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.ChangeAnswersPage("", _sectionSlug, _changeAnswersRouter, _exceptionHandler));
        }

        [Fact]
        public async Task ChangeAnswersPage_Should_Call_ValidateRoute_With_Correct_Parameters()
        {
            var result = await _controller.ChangeAnswersPage(_categorySlug, _sectionSlug, _changeAnswersRouter, _exceptionHandler);

            await _changeAnswersRouter.Received().ValidateRoute(_categorySlug, _sectionSlug, null, _controller, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ChangeAnswersPage_Should_Call_ExceptionHandler_If_UserJourneyException_Thrown()
        {
            var exception = new UserJourneyMissingContentException("fail", Substitute.For<ISectionComponent>());

            _changeAnswersRouter.ValidateRoute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), _controller, Arg.Any<CancellationToken>())
                .Throws(exception);

            await _controller.ChangeAnswersPage(_categorySlug, _sectionSlug, _changeAnswersRouter, _exceptionHandler);

            await _exceptionHandler.Received().Handle(_controller, exception, Arg.Any<CancellationToken>());
        }

        [Fact]
        public void RedirectToRecommendation_Should_Redirect_To_Expected_Route()
        {
            var result = _controller.RedirectToCategoryLandingPage(_categorySlug);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(PagesController.ControllerName, redirect.ControllerName);
            Assert.Equal(PagesController.GetPageByRouteAction, redirect.ActionName);
            Assert.Equal(_categorySlug, redirect.RouteValues?["categorySlug"]);
        }
    }
}
