using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public class RouteToSingleRecommendationMatTests : RecommendationsViewBuilderTestBase
{
    private const int SectionIncludeLevel = 2;

    private static readonly SelectedEstablishmentDataModel AlphaSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(1)
            .WithName("Alpha School")
            .Build();

    private static readonly SelectedEstablishmentDataModel BetaSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(2)
            .WithName("beta School")
            .Build();

    private static readonly SelectedEstablishmentDataModel GammaSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(3)
            .WithName("Gamma School")
            .Build();

    [Fact]
    public async Task RouteToSingleRecommendation_WhenUserIsNotMat_ThrowsInvalidOperationException()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(false);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                FirstChunkSlug,
                CategoryLandingContext.MAT
            )
        );
        Assert.Equal("Non-group users cannot be routed to a MAT page.", exception.Message);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenCategoryIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            null,
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
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
    public async Task RouteToSingleRecommendation_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            null,
            SectionIncludeLevel
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                FirstChunkSlug,
                CategoryLandingContext.MAT
            )
        );
        Assert.Equal($"Could not find section for slug {SectionSlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSingleRecommendation(
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
    public async Task RouteToSingleRecommendation_WhenNoSchoolHasCompletedSection_RedirectsToSelectSchoolsToAssess()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool, BetaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1, 2 }))
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            FirstChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal(GroupsController.GetSelectSchoolsToAssessAction, redirect.RouteName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenOnlyOtherSectionsAreCompleted_RedirectsToSelectSchoolsToAssess()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 }))
            )
            .Returns([
                new SqlSubmissionDto { EstablishmentId = 1, SectionId = "other-section-id" },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            FirstChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        Assert.IsType<RedirectToRouteResult>(result);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenCompletedSubmissionBelongsToSchoolOutsideGroup_RedirectsToSelectSchoolsToAssess()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 }))
            )
            .Returns([new SqlSubmissionDto { EstablishmentId = 99, SectionId = SectionId }]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            FirstChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        Assert.IsType<RedirectToRouteResult>(result);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenGroupSchoolsAreNull_RedirectsToSelectSchoolsToAssess()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns((List<SelectedEstablishmentDataModel>)null!);
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Is<int[]>(ids => ids.Length == 0))
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            FirstChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        Assert.IsType<RedirectToRouteResult>(result);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenSchoolsHaveCompletedSection_ReturnsMatViewWithSchoolsOrderedByName()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        var category = new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build();
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(category, section, SectionIncludeLevel);
        _currentUserProvider.GetGroupSchoolsAsync().Returns([GammaSchool, BetaSchool, AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1, 2, 3 }))
            )
            .Returns([
                new SqlSubmissionDto { EstablishmentId = 3, SectionId = SectionId },
                new SqlSubmissionDto { EstablishmentId = 1, SectionId = SectionId },
                new SqlSubmissionDto { EstablishmentId = 2, SectionId = SectionId },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SingleRecommendation", view.ViewName);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.True(viewModel.IsMat);
        Assert.Equal("Networking", viewModel.CategoryName);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
        Assert.Same(section, viewModel.Section);
        Assert.Equal(SecondChunkSlug, viewModel.OriginatingSlug);
        Assert.Equal(
            new[] { "Alpha School", "beta School", "Gamma School" },
            viewModel.Schools.Select(school => school.SchoolName).ToArray()
        );
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenSchoolHasLatestHistory_ReturnsSchoolStatusAndLastUpdated()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 }))
            )
            .Returns([new SqlSubmissionDto { EstablishmentId = 1, SectionId = SectionId }]);
        var latestHistory = new SqlEstablishmentRecommendationHistoryDtoBuilder()
            .WithNewStatus(RecommendationStatus.Complete)
            .WithDateCreated(new DateTime(2025, 11, 14, 9, 0, 0, DateTimeKind.Utc))
            .Build();
        _recommendationService
            .GetLatestRecommendationHistoryAsync(1, "chunk-2")
            .Returns(latestHistory);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        var school = Assert.Single(viewModel.Schools);
        Assert.Equal(1, school.EstablishmentId);
        Assert.Equal(RecommendationStatus.Complete, school.Status);
        Assert.Equal(latestHistory.DateCreated, school.LastUpdated);
        Assert.False(school.RequiresSelfAssessment);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenCompletedSchoolHasNoHistory_ReturnsNotStartedStatus()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 }))
            )
            .Returns([new SqlSubmissionDto { EstablishmentId = 1, SectionId = SectionId }]);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(1, "chunk-2")
            .Returns((SqlEstablishmentRecommendationHistoryDto?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        var school = Assert.Single(viewModel.Schools);
        Assert.Equal(RecommendationStatus.NotStarted, school.Status);
        Assert.Null(school.LastUpdated);
        Assert.False(school.RequiresSelfAssessment);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenSchoolHasNotCompletedSection_ReturnsSchoolRequiringSelfAssessment()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool, GammaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1, 3 }))
            )
            .Returns([new SqlSubmissionDto { EstablishmentId = 1, SectionId = SectionId }]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        var incompleteSchool = Assert.Single(viewModel.Schools, school =>
            school.EstablishmentId == GammaSchool.EstablishmentId
        );
        Assert.True(incompleteSchool.RequiresSelfAssessment);
        Assert.Null(incompleteSchool.Status);
        Assert.Null(incompleteSchool.LastUpdated);
        await _recommendationService
            .DidNotReceive()
            .GetLatestRecommendationHistoryAsync(GammaSchool.EstablishmentId, Arg.Any<string>());
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkIsInMiddleOfSection_ReturnsMatViewWithNavigationChunksAndMessage()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 }))
            )
            .Returns([new SqlSubmissionDto { EstablishmentId = 1, SectionId = SectionId }]);
        var controller = new TestController();
        controller.TempData[StatePassingMechanismConstants.StatusUpdateSuccessTitle] =
            "Recommendation updated";
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            controller,
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal(SecondChunkSlug, viewModel.CurrentChunk.Slug);
        Assert.Equal(FirstChunkSlug, viewModel.PreviousChunk!.Slug);
        Assert.Equal(ThirdChunkSlug, viewModel.NextChunk!.Slug);
        Assert.Equal(2, viewModel.CurrentChunkPosition);
        Assert.Equal(3, viewModel.TotalChunks);
        Assert.Equal("Recommendation updated", viewModel.SuccessMessageTitle);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_WhenChunkHasRelatedActions_ReturnsRelatedActionsInMatView()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        var chunk = new RecommendationChunkEntryBuilder()
            .WithRelatedActions(
                new RelatedActionEntry { Title = "Read the guidance", Url = "/guidance" }
            )
            .Build();
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().WithCoreRecommendations(chunk).Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 }))
            )
            .Returns([new SqlSubmissionDto { EstablishmentId = 1, SectionId = SectionId }]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSingleRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            FirstChunkSlug,
            CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        var relatedAction = Assert.Single(viewModel.RelatedActions);
        Assert.Equal("Read the guidance", relatedAction.Text);
        Assert.Equal("/guidance", relatedAction.Url);
    }
}
