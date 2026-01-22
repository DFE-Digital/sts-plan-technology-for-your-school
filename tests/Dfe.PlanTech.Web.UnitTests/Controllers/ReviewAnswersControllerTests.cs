using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Handlers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class ReviewAnswersControllerTests
    {
        private readonly ILogger<ReviewAnswersController> _logger = Substitute.For<
            ILogger<ReviewAnswersController>
        >();
        private readonly IReviewAnswersViewBuilder _viewBuilder =
            Substitute.For<IReviewAnswersViewBuilder>();
        private readonly IUserJourneyMissingContentExceptionHandler _exceptionHandler =
            Substitute.For<IUserJourneyMissingContentExceptionHandler>();
        private readonly ReviewAnswersController _controller;

        public ReviewAnswersControllerTests()
        {
            _controller = new ReviewAnswersController(_logger, _exceptionHandler, _viewBuilder);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
        }

        [Fact]
        public void Constructor_WithNullViewBuilder_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new ReviewAnswersController(_logger, _exceptionHandler, null!)
            );

            Assert.Equal("reviewAnswersViewBuilder", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullExceptionHandler_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new ReviewAnswersController(_logger, null!, _viewBuilder)
            );

            Assert.Equal("userJourneyMissingContentExceptionHandler", ex.ParamName);
        }

        [Fact]
        public async Task CheckAnswers_ReturnsExpectedResult()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            _controller.TempData["ErrorMessage"] = "error";
            _viewBuilder
                .RouteToCheckAnswers(_controller, categorySlug, sectionSlug, "error")
                .Returns(new OkResult());

            var result = await _controller.CheckAnswers(categorySlug, sectionSlug);

            await _viewBuilder
                .Received(1)
                .RouteToCheckAnswers(_controller, categorySlug, sectionSlug, "error");
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CheckAnswers_WhenExceptionThrown_UsesExceptionHandler()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            var exception = new UserJourneyMissingContentException(
                "Missing content",
                new QuestionnaireSectionEntry()
            );
            _viewBuilder
                .RouteToCheckAnswers(_controller, categorySlug, sectionSlug, null)
                .Throws(exception);

            _exceptionHandler.Handle(_controller, exception).Returns(new BadRequestResult());

            var result = await _controller.CheckAnswers(categorySlug, sectionSlug);

            await _exceptionHandler.Received(1).Handle(_controller, exception);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ConfirmCheckAnswers_ReturnsExpectedResult()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";
            var sectionName = "Section A";
            var submissionId = 1;

            _viewBuilder
                .ConfirmCheckAnswers(
                    _controller,
                    categorySlug,
                    sectionSlug,
                    sectionName,
                    submissionId
                )
                .Returns(new OkResult());

            var result = await _controller.ConfirmCheckAnswers(
                categorySlug,
                sectionSlug,
                sectionName,
                submissionId
            );

            await _viewBuilder
                .Received(1)
                .ConfirmCheckAnswers(
                    _controller,
                    categorySlug,
                    sectionSlug,
                    sectionName,
                    submissionId
                );
            Assert.IsType<OkResult>(result);
            Assert.Equal("Section A", _controller.TempData["SectionName"]);
        }

        [Fact]
        public async Task ConfirmCheckAnswers_WhenExceptionThrown_LogsErrorAndThrows()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";
            var sectionName = "Section A";
            var submissionId = 1;

            var exception = new Exception("Something went wrong");
            _viewBuilder
                .ConfirmCheckAnswers(
                    _controller,
                    categorySlug,
                    sectionSlug,
                    sectionName,
                    submissionId
                )
                .Throws(exception);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _controller.ConfirmCheckAnswers(
                    categorySlug,
                    sectionSlug,
                    sectionName,
                    submissionId
                )
            );

            Assert.Equal("Something went wrong", ex.Message);
        }

        [Fact]
        public async Task ViewAnswers_ReturnsExpectedResult()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            _controller.TempData["ErrorMessage"] = "error";
            _viewBuilder
                .RouteToViewAnswers(_controller, categorySlug, sectionSlug, "error")
                .Returns(new OkResult());

            var result = await _controller.ViewAnswers(categorySlug, sectionSlug);

            await _viewBuilder
                .Received(1)
                .RouteToViewAnswers(_controller, categorySlug, sectionSlug, "error");
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ViewAnswers_WhenExceptionThrown_UsesExceptionHandler()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            var exception = new UserJourneyMissingContentException(
                "Missing content",
                new QuestionnaireSectionEntry()
            );
            _viewBuilder
                .RouteToViewAnswers(_controller, categorySlug, sectionSlug, null)
                .Throws(exception);

            _exceptionHandler.Handle(_controller, exception).Returns(new BadRequestResult());

            var result = await _controller.ViewAnswers(categorySlug, sectionSlug);

            await _exceptionHandler.Received(1).Handle(_controller, exception);
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
