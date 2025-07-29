using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
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

    [Theory]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData(null)]
    public async Task RecommendationsPage_Preview_Should_Call_RecommendationsRouter_When_Args_Valid(string? maturity)
    {
        string sectionSlug = "section-slug";

        await _recommendationsController.GetRecommendationPreview(sectionSlug, maturity, new ContentfulOptions(true), _recommendationsRouter, default);

        await _recommendationsRouter.Received().GetRecommendationPreview(sectionSlug, maturity, _recommendationsController, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecommendationsPage_Preview_Should_Return_Redirect_When_UsePreview_Is_False()
    {
        string sectionSlug = "section-slug";

        var result = await _recommendationsController.GetRecommendationPreview(sectionSlug, null, new ContentfulOptions(false), _recommendationsRouter, default);

        var redirectResult = result as RedirectResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(UrlConstants.HomePage, redirectResult.Url);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RecommendationsPage_Preview_Should_ThrowException_When_Args_Invalid(string? sectionSlug)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetRecommendationPreview(sectionSlug!, null, new ContentfulOptions(true), _recommendationsRouter, default));
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
