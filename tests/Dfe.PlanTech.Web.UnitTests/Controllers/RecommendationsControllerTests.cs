using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class RecommendationsControllerTests
{
    private readonly IGetRecommendationRouter _recommendationsRouter;

    private readonly RecommendationsController _recommendationsController;

    public RecommendationsControllerTests()
    {
        _recommendationsRouter = Substitute.For<IGetRecommendationRouter>();

        var loggerSubstitute = Substitute.For<ILogger<RecommendationsController>>();
        _recommendationsController = new RecommendationsController(loggerSubstitute);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RecommendationsPagePage_Should_ThrowException_When_SectionSlug_NullOrEmpty(string? section)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetRecommendation(section!, "recommendation", _recommendationsRouter, default));
    }


    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RecommendationsPagePage_Should_ThrowException_When_RecommendationSlug_NullOrEmpty(string? recommendation)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetRecommendation("section slug", recommendation!, _recommendationsRouter, default));
    }

    [Fact]
    public async Task RecommendationsPagePage_Should_Call_RecommendationsRouter_When_Args_Valid()
    {
        string sectionSlug = "section-slug";
        string recommendationSlug = "recommendation-slug";

        await _recommendationsController.GetRecommendation(sectionSlug, recommendationSlug, _recommendationsRouter, default);

        await _recommendationsRouter.Received().ValidateRoute(sectionSlug, recommendationSlug, false, _recommendationsController, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecommendationsPagePageChecklist_Should_Call_RecommendationsRouter_When_Args_Valid()
    {
        string sectionSlug = "section-slug";
        string recommendationSlug = "recommendation-slug";

        await _recommendationsController.GetRecommendationChecklist(sectionSlug, recommendationSlug, _recommendationsRouter, default);

        await _recommendationsRouter.Received().ValidateRoute(sectionSlug, recommendationSlug, true, _recommendationsController, Arg.Any<CancellationToken>());
    }

}