using System.Data;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Interfaces;
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
    private readonly IGetPageQuery _getPageQuery;

    private readonly RecommendationsController _recommendationsController;

    public RecommendationsControllerTests()
    {
        _recommendationsRouter = Substitute.For<IGetRecommendationRouter>();

        var loggerSubstitute = Substitute.For<ILogger<RecommendationsController>>();
        _recommendationsController = new RecommendationsController(loggerSubstitute);
        _getPageQuery = Substitute.For<IGetPageQuery>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetSingleRecommendation_Should_ThrowException_When_CategorySlug_NullOrEmpty(string? category)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetSingleRecommendation(category!, "section-slug", "recommendation", _recommendationsRouter, _getPageQuery, default));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetSingleRecommendation_Should_ThrowException_When_SectionSlug_NullOrEmpty(string? section)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetSingleRecommendation("category-slug", section!, "recommendation", _recommendationsRouter, _getPageQuery, default));
    }


    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetSingleRecommendation_Should_ThrowException_When_RecommendationSlug_NullOrEmpty(string? recommendation)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetSingleRecommendation("category-slug", "section slug", recommendation!, _recommendationsRouter, _getPageQuery, default));
    }

    [Fact]
    public async Task GetSingleRecommendation_Should_Call_RecommendationsRouter_When_Args_Valid()
    {
        string categorySlug = "categorySlug";
        string sectionSlug = "section-slug";
        string recommendationSlug = "recommendation-slug";

        await _recommendationsController.GetSingleRecommendation(categorySlug, sectionSlug, recommendationSlug, _recommendationsRouter, _getPageQuery, default);

        await _recommendationsRouter.Received().ValidateRoute(categorySlug, sectionSlug, false, _recommendationsController, Arg.Any<CancellationToken>());
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
        string categorySlug = "category-slug";
        string sectionSlug = "section-slug";

        await _recommendationsController.GetRecommendationChecklist(categorySlug, sectionSlug, _recommendationsRouter, default);

        await _recommendationsRouter.Received().ValidateRoute(categorySlug, sectionSlug, true, _recommendationsController, Arg.Any<CancellationToken>());
    }
}
