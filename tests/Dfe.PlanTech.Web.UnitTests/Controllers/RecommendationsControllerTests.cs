using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.Tests.Controllers
{
    public class RecommendationsControllerTests
    {
        private readonly ILogger<RecommendationsController> _logger;
        private readonly IRecommendationsViewBuilder _viewBuilder;
        private readonly IRecommendationService _recommendationService;
        private readonly IContentfulService _contentfulService;
        private readonly ISubmissionService _submissionService;
        private readonly ICurrentUser _currentUser;
        private readonly RecommendationsController _controller;

        public RecommendationsControllerTests()
        {
            _logger = Substitute.For<ILogger<RecommendationsController>>();
            _viewBuilder = Substitute.For<IRecommendationsViewBuilder>();
            _recommendationService = Substitute.For<IRecommendationService>();
            _contentfulService = Substitute.For<IContentfulService>();
            _submissionService = Substitute.For<ISubmissionService>();
            _currentUser = Substitute.For<ICurrentUser>();
            _controller = new RecommendationsController(_logger, _contentfulService, _recommendationService, _viewBuilder, _submissionService, _currentUser);
        }

        [Fact]
        public void Constructor_WithNullViewBuilder_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new RecommendationsController(_logger, _contentfulService, _recommendationService, null!, _submissionService, _currentUser)
            );

            Assert.Equal("recommendationsViewBuilder", ex.ParamName);
        }

        [Fact]
        public async Task GetSingleRecommendation_CallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";
            var chunkSlug = "chunk";

            _viewBuilder.RouteToSingleRecommendation(_controller, categorySlug, sectionSlug, chunkSlug, false)
                .Returns(new OkResult());

            var result = await _controller.GetSingleRecommendation(categorySlug, sectionSlug, chunkSlug);

            await _viewBuilder.Received(1).RouteToSingleRecommendation(_controller, categorySlug, sectionSlug, chunkSlug, false);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetRecommendationChecklist_CallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";

            _viewBuilder.RouteBySectionAndRecommendation(_controller, categorySlug, sectionSlug, true)
                .Returns(new OkResult());

            var result = await _controller.GetRecommendationChecklist(categorySlug, sectionSlug);

            await _viewBuilder.Received(1).RouteBySectionAndRecommendation(_controller, categorySlug, sectionSlug, true);
            Assert.IsType<OkResult>(result);
        }
    }
}
