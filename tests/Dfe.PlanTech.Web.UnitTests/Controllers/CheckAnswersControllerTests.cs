using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CheckAnswersControllerTests
    {
        private readonly CheckAnswersController _checkAnswersController;
        private readonly ICalculateMaturityCommand _calculateMaturityCommand;
        private readonly ICheckAnswersRouter _checkAnswersRouter;

        public CheckAnswersControllerTests()
        {
            var loggerSubstitute = Substitute.For<ILogger<CheckAnswersController>>();
            _calculateMaturityCommand = Substitute.For<ICalculateMaturityCommand>();
            _checkAnswersRouter = Substitute.For<ICheckAnswersRouter>();

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
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(section!, _checkAnswersRouter, default));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task ConfirmAnswers_Should_ThrowException_When_SubmissionId_OutOfRange(int submissionId)
        {
            await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(() => _checkAnswersController.ConfirmCheckAnswers(submissionId, "section name", _calculateMaturityCommand));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ConfirmAnswers_Should_ThrowException_When_SectionName_NullOrEmpty(string? sectionName)
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.ConfirmCheckAnswers(1, sectionName!, _calculateMaturityCommand));
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

            var result = await _checkAnswersController.ConfirmCheckAnswers(submissionId, "section name", _calculateMaturityCommand);

            Assert.Equal(submissionId, submissionIdResult);
        }

        [Fact]
        public async Task ConfirmAnswers_Should_Redirect_To_SelfAssessmentPage()
        {
            var result = await _checkAnswersController.ConfirmCheckAnswers(1, "section name", _calculateMaturityCommand);

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
    }
}
