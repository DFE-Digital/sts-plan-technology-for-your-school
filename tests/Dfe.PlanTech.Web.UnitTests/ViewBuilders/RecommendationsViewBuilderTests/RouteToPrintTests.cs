using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public class RouteToPrintTests : RecommendationsViewBuilderTestBase
{
    private const int SectionIncludeLevel = 2;

    private static readonly Dictionary<
        string,
        SqlEstablishmentRecommendationHistoryDto
    > LatestStatuses = new()
    {
        ["chunk-1"] = new SqlEstablishmentRecommendationHistoryDtoBuilder().Build(),
        ["chunk-2"] = new SqlEstablishmentRecommendationHistoryDtoBuilder().Build(),
        ["chunk-3"] = new SqlEstablishmentRecommendationHistoryDtoBuilder().Build(),
    };

    [Fact]
    public async Task RouteToPrintSingle_WhenContextIsSchool_ReturnsChecklistViewWithOnlyRequestedChunk()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        ArrangeCompleteReviewedSubmission(section);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToPrintSingle(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RecommendationsChecklist", view.ViewName);
        var viewModel = Assert.IsType<RecommendationsViewModel>(view.Model);
        var chunk = Assert.Single(viewModel.Chunks);
        Assert.Equal(SecondChunkSlug, chunk.Slug);
        Assert.Equal(2, viewModel.CurrentChunkCount);
        Assert.Equal(3, viewModel.TotalChunks);
        Assert.Equal(SecondChunkSlug, viewModel.OriginatingSlug);
        Assert.False(viewModel.IsMat);
    }

    [Fact]
    public async Task RouteToPrintAll_WhenContextIsSchool_ReturnsChecklistViewWithAllChunks()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        ArrangeCompleteReviewedSubmission(section);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToPrintAll(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RecommendationsChecklist", view.ViewName);
        var viewModel = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.Equal(
            new[] { FirstChunkSlug, SecondChunkSlug, ThirdChunkSlug },
            viewModel.Chunks.Select(chunk => chunk.Slug).ToArray()
        );
        Assert.Null(viewModel.CurrentChunkCount);
        Assert.Equal(SecondChunkSlug, viewModel.OriginatingSlug);
        Assert.False(viewModel.IsMat);
    }

    [Fact]
    public async Task RouteToPrintSingle_WhenContextIsMat_ReturnsMatChecklistViewWithOnlyRequestedChunk()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build();
        var section = new QuestionnaireSectionEntryBuilder().WithName("Network security").Build();
        ArrangeContentful(category, section, SectionIncludeLevel);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToPrintSingle(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RecommendationsChecklist", view.ViewName);
        var viewModel = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.True(viewModel.IsMat);
        Assert.Equal("Networking", viewModel.CategoryName);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal("Network security", viewModel.SectionName);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
        Assert.Equal(SecondChunkSlug, viewModel.OriginatingSlug);
        Assert.Equal(2, viewModel.CurrentChunkCount);
        Assert.Equal(3, viewModel.TotalChunks);
        var chunk = Assert.Single(viewModel.Chunks);
        Assert.Equal(SecondChunkSlug, chunk.Slug);
        Assert.Equal("Second chunk", chunk.Header);
        Assert.Equal("text-body-2", Assert.Single(chunk.Content).Id);
        await _currentUserProvider.DidNotReceive().GetFocusedEstablishmentAsync();
    }

    [Fact]
    public async Task RouteToPrintAll_WhenContextIsMat_ReturnsMatChecklistViewWithAllChunks()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToPrintAll(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RecommendationsChecklist", view.ViewName);
        var viewModel = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.True(viewModel.IsMat);
        Assert.Equal(
            new[] { FirstChunkSlug, SecondChunkSlug, ThirdChunkSlug },
            viewModel.Chunks.Select(chunk => chunk.Slug).ToArray()
        );
        Assert.Null(viewModel.CurrentChunkCount);
        Assert.Equal(3, viewModel.TotalChunks);
        Assert.Equal(SecondChunkSlug, viewModel.OriginatingSlug);
        await _currentUserProvider.DidNotReceive().GetFocusedEstablishmentAsync();
    }

    [Fact]
    public async Task RouteToPrintSingle_WhenContextIsMatAndChunkIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToPrintSingle(
                new TestController(),
                CategorySlug,
                SectionSlug,
                "missing-chunk",
                CategoryLandingContext.MAT
            )
        );
        Assert.Equal(
            "No recommendation chunk found with slug matching: missing-chunk",
            exception.Message
        );
    }

    [Fact]
    public async Task RouteToPrintSingle_WhenContextIsMatAndCategoryIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            null,
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToPrintSingle(
                new TestController(),
                CategorySlug,
                SectionSlug,
                FirstChunkSlug,
                CategoryLandingContext.MAT
            )
        );
        Assert.Equal($"Could not find category for slug {CategorySlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToPrintAll_WhenContextIsMatAndSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            null,
            SectionIncludeLevel
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToPrintAll(
                new TestController(),
                CategorySlug,
                SectionSlug,
                FirstChunkSlug,
                CategoryLandingContext.MAT
            )
        );
        Assert.Equal($"Could not find section for slug {SectionSlug}", exception.Message);
    }

    private void ArrangeCompleteReviewedSubmission(QuestionnaireSectionEntry section)
    {
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    new SubmissionResponsesModel(1, []),
                    SubmissionStatus.CompleteReviewed
                )
            );
        _recommendationService
            .GetLatestRecommendationStatusesAsync(FocusedEstablishmentId)
            .Returns(LatestStatuses);
    }
}
