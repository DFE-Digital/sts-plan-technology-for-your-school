using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class RecommendationsControllerTests
    {
        private readonly ILogger<RecommendationsController> _logger;
        private readonly IRecommendationsViewBuilder _viewBuilder;
        private readonly RecommendationsController _controller;

        public RecommendationsControllerTests()
        {
            _logger = Substitute.For<ILogger<RecommendationsController>>();
            _viewBuilder = Substitute.For<IRecommendationsViewBuilder>();
            _controller = new RecommendationsController(_logger, _viewBuilder);
        }

        [Fact]
        public void Constructor_WithNullViewBuilder_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new RecommendationsController(_logger, null!)
            );

            Assert.Equal("recommendationsViewBuilder", ex.ParamName);
        }

        [Fact]
        public async Task GetSingleRecommendation_CallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";
            var chunkSlug = "chunk";

            _viewBuilder
                .RouteToSingleRecommendation(
                    _controller,
                    categorySlug,
                    sectionSlug,
                    chunkSlug,
                    false
                )
                .Returns(new OkResult());

            var result = await _controller.GetSingleRecommendation(
                categorySlug,
                sectionSlug,
                chunkSlug
            );

            await _viewBuilder
                .Received(1)
                .RouteToSingleRecommendation(
                    _controller,
                    categorySlug,
                    sectionSlug,
                    chunkSlug,
                    false
                );
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task PrintSingleRecommendation_CallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";
            var chunkSlug = "rec-1";

            _viewBuilder
                .RouteToPrintSingle(_controller, categorySlug, sectionSlug, chunkSlug)
                .Returns(new OkResult());

            var result = await _controller.PrintSingleRecommendation(
                categorySlug,
                sectionSlug,
                chunkSlug
            );

            await _viewBuilder
                .Received(1)
                .RouteToPrintSingle(_controller, categorySlug, sectionSlug, chunkSlug);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task PrintAllRecommendations_CallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "cat";
            var sectionSlug = "sec";
            var chunkSlug = "rec-1";

            _viewBuilder
                .RouteToPrintAll(_controller, categorySlug, sectionSlug, chunkSlug)
                .Returns(new OkResult());

            var result = await _controller.PrintAllRecommendations(
                categorySlug,
                sectionSlug,
                chunkSlug
            );

            await _viewBuilder
                .Received(1)
                .RouteToPrintAll(_controller, categorySlug, sectionSlug, chunkSlug);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateRecommendationStatus_ThrowsIfCategorySlugNull()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.UpdateRecommendationStatus(
                    null!,
                    "section-slug",
                    "chunk-slug",
                    RecommendationStatus.Complete.ToString(),
                    null
                )
            );
        }

        [Fact]
        public async Task UpdateRecommendationStatus_ThrowsIfSectionSlugNull()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.UpdateRecommendationStatus(
                    "category-slug",
                    null!,
                    "chunk-slug",
                    RecommendationStatus.Complete.ToString(),
                    null
                )
            );
        }

        [Fact]
        public async Task UpdateRecommendationStatus_ThrowsIfChunkSlugNull()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.UpdateRecommendationStatus(
                    "category-slug",
                    "section-slug",
                    null!,
                    RecommendationStatus.Complete.ToString(),
                    null
                )
            );
        }
    }
}
