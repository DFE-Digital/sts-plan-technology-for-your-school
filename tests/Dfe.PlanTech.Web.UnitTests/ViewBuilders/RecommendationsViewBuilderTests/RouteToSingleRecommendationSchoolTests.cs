using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public class RouteToSingleRecommendationSchoolTests : RecommendationsViewBuilderTestBase
{
    private const int SectionIncludeLevel = 2;

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkIsInMiddleOfSection_ReturnsViewWithNavigationChunks()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build();
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(category, section, SectionIncludeLevel);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            controller,
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SingleRecommendation", view.ViewName);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal("Networking", viewModel.CategoryName);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
        Assert.Same(section, viewModel.Section);
        Assert.Equal(SecondChunkSlug, viewModel.OriginatingSlug);
        Assert.Equal(SecondChunkSlug, viewModel.CurrentChunk.Slug);
        Assert.Equal(FirstChunkSlug, viewModel.PreviousChunk!.Slug);
        Assert.Equal(ThirdChunkSlug, viewModel.NextChunk!.Slug);
        Assert.Equal(2, viewModel.CurrentChunkPosition);
        Assert.Equal(3, viewModel.TotalChunks);
        Assert.Equal(
            new[] { FirstChunkSlug, SecondChunkSlug, ThirdChunkSlug },
            viewModel.Chunks.Select(chunk => chunk.Slug).ToArray()
        );
        Assert.False(viewModel.IsMat);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkIsFirstInSection_ReturnsViewWithoutPreviousChunk()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            FirstChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Null(viewModel.PreviousChunk);
        Assert.Equal(SecondChunkSlug, viewModel.NextChunk!.Slug);
        Assert.Equal(1, viewModel.CurrentChunkPosition);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkIsLastInSection_ReturnsViewWithoutNextChunk()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            ThirdChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal(SecondChunkSlug, viewModel.PreviousChunk!.Slug);
        Assert.Null(viewModel.NextChunk);
        Assert.Equal(3, viewModel.CurrentChunkPosition);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenLatestHistoryExists_ReturnsStatusAndLastUpdated()
    {
        // Arrange
        var latestHistory = new SqlEstablishmentRecommendationHistoryDtoBuilder()
            .WithNewStatus(RecommendationStatus.Complete)
            .WithDateCreated(new DateTime(2025, 11, 14, 9, 0, 0, DateTimeKind.Utc))
            .Build();
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(FocusedEstablishmentId, "chunk-2")
            .Returns(latestHistory);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal(RecommendationStatus.Complete, viewModel.SelectedStatusKey);
        Assert.Equal(latestHistory.DateCreated, viewModel.LastUpdated);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenNoHistoryExists_ReturnsNotStartedStatusWithoutLastUpdated()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(FocusedEstablishmentId, "chunk-2")
            .Returns((SqlEstablishmentRecommendationHistoryDto?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal(RecommendationStatus.NotStarted, viewModel.SelectedStatusKey);
        Assert.Null(viewModel.LastUpdated);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenHistoryExists_ReturnsHistoryGroupedByMonthNewestFirst()
    {
        // Arrange
        var novemberLatest = new SqlEstablishmentRecommendationHistoryDtoBuilder()
            .WithDateCreated(new DateTime(2025, 11, 14))
            .Build();
        var novemberEarlier = new SqlEstablishmentRecommendationHistoryDtoBuilder()
            .WithDateCreated(new DateTime(2025, 11, 11))
            .Build();
        var october = new SqlEstablishmentRecommendationHistoryDtoBuilder()
            .WithDateCreated(new DateTime(2025, 10, 5))
            .Build();
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetRecommendationHistoryAsync("chunk-2", FocusedEstablishmentId)
            .Returns([novemberEarlier, october, novemberLatest]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal(new[] { "November 2025", "October 2025" }, viewModel.History.Keys.ToArray());
        Assert.Equal(
            new[] { novemberLatest, novemberEarlier },
            viewModel.History["November 2025"].ToArray()
        );
        Assert.Equal(new[] { october }, viewModel.History["October 2025"].ToArray());
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenFirstActivityExists_ReturnsFirstActivity()
    {
        // Arrange
        var firstActivity = new SqlFirstActivityForEstablishmentRecommendationDto
        {
            SchoolName = "Springfield Primary",
            Status = RecommendationStatus.InProgress,
        };
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetFirstActivityForEstablishmentRecommendationAsync(FocusedEstablishmentId, "chunk-2")
            .Returns(firstActivity);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Same(firstActivity, viewModel.FirstActivity);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenTempDataContainsMessages_ReturnsMessagesInViewModel()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var controller = new TestController();
        controller.TempData[StatePassingMechanismConstants.StatusUpdateSuccessTitle] =
            "Recommendation updated";
        controller.TempData[StatePassingMechanismConstants.StatusUpdateError] =
            "Status could not be updated";
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            controller,
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal("Recommendation updated", viewModel.SuccessMessageTitle);
        Assert.Equal("Status could not be updated", viewModel.StatusErrorMessage);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenCalled_ReturnsAllStatusOptionsWithDisplayNames()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var expectedStatusOptions = new Dictionary<RecommendationStatus, string>
        {
            [RecommendationStatus.NotStarted] = "Not\u00A0started",
            [RecommendationStatus.InProgress] = "In\u00A0progress",
            [RecommendationStatus.Complete] = "Complete",
        };
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal(expectedStatusOptions, viewModel.StatusOptions);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkHasRelatedActions_ReturnsRelatedActionsSkippingNullEntries()
    {
        // Arrange
        var chunk = new RecommendationChunkEntryBuilder()
            .WithRelatedActions(
                new RelatedActionEntry { Title = "Read the guidance", Url = "/guidance" },
                null!,
                new RelatedActionEntry()
            )
            .Build();
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().WithCoreRecommendations(chunk).Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            FirstChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Collection(
            viewModel.RelatedActions,
            relatedAction =>
            {
                Assert.Equal("Read the guidance", relatedAction.Text);
                Assert.Equal("/guidance", relatedAction.Url);
            },
            relatedAction =>
            {
                Assert.Equal(string.Empty, relatedAction.Text);
                Assert.Equal(string.Empty, relatedAction.Url);
            }
        );
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkHasNoRelatedActions_ReturnsEmptyRelatedActions()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            FirstChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Empty(viewModel.RelatedActions);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenCategoryIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            null,
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                FirstChunkSlug
            )
        );
        Assert.Equal($"Could not find category for slug {CategorySlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenCategoryHasNoHeader_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().WithoutHeader().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                FirstChunkSlug
            )
        );
        Assert.Equal(
            $"Could not find category header text for slug {CategorySlug}",
            exception.Message
        );
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            null,
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                FirstChunkSlug
            )
        );
        Assert.Equal($"Could not find section for slug {SectionSlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                "missing-chunk"
            )
        );
        Assert.Equal(
            "No recommendation chunk found with slug matching: missing-chunk",
            exception.Message
        );
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenContextIsNotSupported_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                FirstChunkSlug,
                (CategoryLandingContext)99
            )
        );
        Assert.Equal("context", exception.ParamName);
    }
}
