using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class QuestionsControllerTests
    {
        private readonly QuestionsController _controller;
        private readonly IQuestionsViewBuilder _viewBuilder =
            Substitute.For<IQuestionsViewBuilder>();
        private readonly ILogger<QuestionsController> _logger = Substitute.For<
            ILogger<QuestionsController>
        >();

        public QuestionsControllerTests()
        {
            _controller = new QuestionsController(_logger, _viewBuilder);
        }

        [Fact]
        public async Task GetQuestionBySlug_CallsViewBuilderWithCorrectParameters()
        {
            var categorySlug = "category";
            var sectionSlug = "section";
            var questionSlug = "question";
            var returnTo = "return";

            _viewBuilder
                .RouteBySlugAndQuestionAsync(
                    _controller,
                    categorySlug,
                    sectionSlug,
                    questionSlug,
                    returnTo
                )
                .Returns(new OkResult());

            var result = await _controller.GetQuestionBySlug(
                categorySlug,
                sectionSlug,
                questionSlug,
                returnTo
            );

            await _viewBuilder
                .Received(1)
                .RouteBySlugAndQuestionAsync(
                    _controller,
                    categorySlug,
                    sectionSlug,
                    questionSlug,
                    returnTo
                );
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetInterstitialPage_CallsViewBuilder()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            _viewBuilder
                .RouteToInterstitialPage(_controller, categorySlug, sectionSlug)
                .Returns(new OkResult());

            var result = await _controller.GetInterstitialPage(categorySlug, sectionSlug);

            await _viewBuilder
                .Received(1)
                .RouteToInterstitialPage(_controller, categorySlug, sectionSlug);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetQuestionPreviewById_CallsViewBuilder()
        {
            var questionId = "123";

            _viewBuilder.RouteByQuestionId(_controller, questionId).Returns(new OkResult());

            var result = await _controller.GetQuestionPreviewById(questionId);

            await _viewBuilder.Received(1).RouteByQuestionId(_controller, questionId);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetNextUnansweredQuestion_CallsViewBuilder()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            _viewBuilder
                .RouteToNextUnansweredQuestion(_controller, categorySlug, sectionSlug)
                .Returns(new OkResult());

            var result = await _controller.GetNextUnansweredQuestion(categorySlug, sectionSlug);

            await _viewBuilder
                .Received(1)
                .RouteToNextUnansweredQuestion(_controller, categorySlug, sectionSlug);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SubmitAnswer_CallsViewBuilder()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";
            var questionSlug = "q";
            var returnTo = "return";
            var answerViewModel = new SubmitAnswerInputViewModel();

            _viewBuilder
                .SubmitAnswerAndRedirect(
                    _controller,
                    answerViewModel,
                    categorySlug,
                    sectionSlug,
                    questionSlug,
                    returnTo
                )
                .Returns(new OkResult());

            var result = await _controller.SubmitAnswer(
                categorySlug,
                sectionSlug,
                questionSlug,
                answerViewModel,
                returnTo
            );

            await _viewBuilder
                .Received(1)
                .SubmitAnswerAndRedirect(
                    _controller,
                    answerViewModel,
                    categorySlug,
                    sectionSlug,
                    questionSlug,
                    returnTo
                );
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetContinueSelfAssessmentPage_CallsViewBuilder()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            _viewBuilder
                .RouteToContinueSelfAssessmentPage(_controller, categorySlug, sectionSlug)
                .Returns(new OkResult());

            var result = await _controller.GetContinueSelfAssessment(categorySlug, sectionSlug);

            await _viewBuilder
                .Received(1)
                .RouteToContinueSelfAssessmentPage(_controller, categorySlug, sectionSlug);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Constructor_WithNullQuestionsViewBuilder_ThrowsArgumentNullException()
        {
            var logger = Substitute.For<ILogger<QuestionsController>>();

            var exception = Assert.Throws<ArgumentNullException>(() =>
                new QuestionsController(logger, null!)
            );

            Assert.Equal("questionsViewBuilder", exception.ParamName);
        }

        [Fact]
        public async Task RestartSelfAssessment_CallsViewBuilder()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            _viewBuilder
                .RestartSelfAssessment(_controller, categorySlug, sectionSlug, false)
                .Returns(new OkResult());

            var result = await _controller.RestartSelfAssessment(categorySlug, sectionSlug, false);

            await _viewBuilder
                .Received(1)
                .RestartSelfAssessment(_controller, categorySlug, sectionSlug, false);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ContinuePreviousAssessment_CallsViewBuilder()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            _viewBuilder
                .ContinuePreviousAssessment(_controller, categorySlug, sectionSlug)
                .Returns(new OkResult());

            var result = await _controller.ContinuePreviousAssessment(categorySlug, sectionSlug);

            await _viewBuilder
                .Received(1)
                .ContinuePreviousAssessment(_controller, categorySlug, sectionSlug);
            Assert.IsType<OkResult>(result);
        }
    }
}
