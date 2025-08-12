using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
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
    private readonly IGetCategoryQuery _getCategoryQuery;

    private readonly RecommendationsController _recommendationsController;

    public RecommendationsControllerTests()
    {
        _recommendationsRouter = Substitute.For<IGetRecommendationRouter>();

        var loggerSubstitute = Substitute.For<ILogger<RecommendationsController>>();
        _recommendationsController = new RecommendationsController(loggerSubstitute);
        _getCategoryQuery = Substitute.For<IGetCategoryQuery>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetSingleRecommendation_Should_ThrowException_When_CategorySlug_NullOrEmpty(string? category)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetSingleRecommendation(category!, "section-slug", "recommendation", _recommendationsRouter, _getCategoryQuery, default));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetSingleRecommendation_Should_ThrowException_When_SectionSlug_NullOrEmpty(string? section)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetSingleRecommendation("category-slug", section!, "recommendation", _recommendationsRouter, _getCategoryQuery, default));
    }


    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetSingleRecommendation_Should_ThrowException_When_RecommendationSlug_NullOrEmpty(string? recommendation)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _recommendationsController.GetSingleRecommendation("category-slug", "section slug", recommendation!, _recommendationsRouter, _getCategoryQuery, default));
    }

    [Fact]
    public async Task GetSingleRecommendation_Should_Call_RecommendationsRouter_When_Args_Valid()
    {
        string categorySlug = "categorySlug";
        string sectionSlug = "section-slug";
        string chunkSlug = "test-chunk-one";

        var category = new Category()
        {
            Header = new Header() { Text = "Test category" }
        };

        var section = new Section() { InterstitialPage = new Page() { Slug = sectionSlug } };
        var testChunk1 = new RecommendationChunk() { Header = "Test chunk one" };
        var testChunk2 = new RecommendationChunk() { Header = "Test chunk two" };
        var testChunk3 = new RecommendationChunk() { Header = "Test chunk three" };
        var allTestChunks = new List<RecommendationChunk> { testChunk1, testChunk2, testChunk3 };

        _getCategoryQuery.GetCategoryBySlug(categorySlug).Returns(category);
        _recommendationsRouter.GetSingleRecommendation(sectionSlug, chunkSlug, _recommendationsController, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((section, testChunk1, allTestChunks)));

        var result = await _recommendationsController.GetSingleRecommendation(
            categorySlug, sectionSlug, chunkSlug, _recommendationsRouter, _getCategoryQuery, CancellationToken.None);

        await _recommendationsRouter.Received().GetSingleRecommendation(
            sectionSlug, chunkSlug, _recommendationsController, Arg.Any<CancellationToken>());
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
