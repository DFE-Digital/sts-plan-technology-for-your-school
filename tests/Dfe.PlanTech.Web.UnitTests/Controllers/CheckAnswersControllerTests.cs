using System.Diagnostics;
using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
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
    public class CheckAnswersControllerTests
    {
        private readonly CheckAnswersController _checkAnswersController;
        private readonly ICalculateMaturityCommand _calculateMaturityCommand;
        private readonly ICheckAnswersRouter _checkAnswersRouter;
        private readonly IUserJourneyMissingContentExceptionHandler _userJourneyMissingContentExceptionHandler;
        private readonly string _sectionSlug = "section-slug";

        public CheckAnswersControllerTests()
        {
            var loggerSubstitute = Substitute.For<ILogger<CheckAnswersController>>();
            _calculateMaturityCommand = Substitute.For<ICalculateMaturityCommand>();
            _checkAnswersRouter = Substitute.For<ICheckAnswersRouter>();
            _userJourneyMissingContentExceptionHandler = Substitute.For<IUserJourneyMissingContentExceptionHandler>();

            _checkAnswersController = new CheckAnswersController(loggerSubstitute)
            {
                ControllerContext = ControllerHelpers.SubstituteControllerContext()
            };
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task CheckAnswersPage_Should_ThrowException_When_SectionSlug_NullOrEmpty(string? section)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => _checkAnswersController.CheckAnswersPage(section!, _checkAnswersRouter, _userJourneyMissingContentExceptionHandler, default));
        }

        [Fact]
        public async Task CheckAnswersPage_Should_Call_CheckAnswersRouter_When_Args_Valid()
        {
            await _checkAnswersController.CheckAnswersPage(_sectionSlug, _checkAnswersRouter,
                _userJourneyMissingContentExceptionHandler, default);
            await _checkAnswersRouter.Received()
                .ValidateRoute(_sectionSlug, null, _checkAnswersController, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CheckAnswersPage_Should_Call_UserJourneyMissingContentExceptionHandler_When_Exception_Thrown()
        {
            var exception = new UserJourneyMissingContentException("Exception thrown", new Section());

            _checkAnswersRouter.ValidateRoute(Arg.Any<string>(), Arg.Any<string>(), _checkAnswersController,
                    Arg.Any<CancellationToken>()).Throws(exception);

             await _checkAnswersController.CheckAnswersPage(_sectionSlug, _checkAnswersRouter,
                _userJourneyMissingContentExceptionHandler, default);

             await _userJourneyMissingContentExceptionHandler.Received().Handle(_checkAnswersController, exception,
                 Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task ConfirmAnswers_Should_ThrowException_When_SubmissionId_OutOfRange(int submissionId)
        {
            await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(() => _checkAnswersController.ConfirmCheckAnswers(_sectionSlug, submissionId, "section name", _calculateMaturityCommand));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ConfirmAnswers_Should_ThrowException_When_SectionName_NullOrEmpty(string? sectionName)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => _checkAnswersController.ConfirmCheckAnswers(_sectionSlug, 1, sectionName!, _calculateMaturityCommand));
        }

        [Fact]
        public async Task ConfirmAnswers_Should_CalculateMaturity_When_ArgsValid()
        {
            int submissionId = 1;
            int? submissionIdResult = null;

            _calculateMaturityCommand.CalculateMaturityAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                                        .Returns((callinfo) =>
                                        {
                                            submissionIdResult = callinfo.ArgAt<int>(0);
                                            return 2;
                                        });

            var result = await _checkAnswersController.ConfirmCheckAnswers(_sectionSlug, submissionId, "section name", _calculateMaturityCommand);

            Assert.Equal(submissionId, submissionIdResult);
        }

        [Fact]
        public async Task ConfirmAnswers_Should_Redirect_To_SelfAssessmentPage()
        {
            var result = await _checkAnswersController.ConfirmCheckAnswers(_sectionSlug, 1, "section name", _calculateMaturityCommand);

            var redirectToActionResult = result as RedirectToActionResult;
            if (redirectToActionResult == null)
            {
                Assert.Fail("Not redirect to action result");
            }

            Assert.Equal(PagesController.ControllerName, redirectToActionResult.ControllerName);
            Assert.Equal(PagesController.GetPageByRouteAction, redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            Assert.True(redirectToActionResult.RouteValues.ContainsKey("route"));
            Assert.True(redirectToActionResult.RouteValues["route"] is string s && s == "/self-assessment");
        }

        [Fact]
        public async Task ConfirmAnswers_Should_Redirect_To_CheckAnswers()
        {
            _calculateMaturityCommand.CalculateMaturityAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Throws(new Exception());

            var result = await _checkAnswersController.ConfirmCheckAnswers(_sectionSlug, 1, "section name", _calculateMaturityCommand);

            var redirectToActionResult = result as RedirectToActionResult;
            if (redirectToActionResult == null)
            {
                Assert.Fail("Not redirect to action result");
            }

            Assert.Equal(CheckAnswersController.ControllerName, redirectToActionResult.ControllerName);
            Assert.Equal(CheckAnswersController.CheckAnswersAction, redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            Debug.Assert(redirectToActionResult.RouteValues != null, "checkAnswerResult.RouteValues != null");
            Assert.Equal(_sectionSlug, redirectToActionResult.RouteValues["sectionSlug"]);
            Assert.Equal("Unable to save. Please try again. If this problem continues you can", _checkAnswersController.TempData["ErrorMessage"]);
        }
    }
}
