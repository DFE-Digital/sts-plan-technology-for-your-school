using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using EstablishmentHistories = System.Collections.Generic.List<(
    Dfe.PlanTech.Data.Sql.Entities.EstablishmentEntity establishment,
    Dfe.PlanTech.Data.Sql.Entities.EstablishmentRecommendationHistoryEntity? recommendationHistory
)>;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class RouteToSelectSchoolsToUpdateStatusViewModelAsyncTests : GroupsViewBuilderTestBase
{
    private const int TrustEstablishmentId = 500;

    private readonly EstablishmentEntity _oakfieldEntity = new EstablishmentEntityBuilder()
        .WithId(11)
        .WithEstablishmentRef("100011")
        .WithOrgName("Oakfield Primary")
        .Build();

    private readonly EstablishmentEntity _riverbankEntity = new EstablishmentEntityBuilder()
        .WithId(12)
        .WithEstablishmentRef("100012")
        .WithOrgName("Riverbank Primary")
        .Build();

    public RouteToSelectSchoolsToUpdateStatusViewModelAsyncTests()
    {
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(TrustEstablishmentId).Build());
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        _recommendationService
            .GetRecommendationsByContentfulReferencesAsync(
                Arg.Is<IEnumerable<string>>(references =>
                    references.SequenceEqual(new[] { RecommendationChunkId })
                )
            )
            .Returns([
                new SqlRecommendationDto { Id = 1, ContentfulSysId = RecommendationChunkId },
            ]);
        _groupService
            .GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRef(
                TrustEstablishmentId,
                RecommendationChunkId
            )
            .Returns(new EstablishmentHistories());
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenCalled_ClearsSelectedSchools()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        _currentUserProvider.Received(1).ClearSelectedSchools();
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenSectionIsMissing_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                new TestController(),
                SectionSlug,
                RecommendationSlug
            )
        );
        Assert.Contains(SectionSlug, exception.Message);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenRecommendationSlugIsNotInSection_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                new TestController(),
                SectionSlug,
                "unknown-recommendation-slug"
            )
        );
        Assert.Contains("unknown-recommendation-slug", exception.Message);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenRecommendationIsNotInDatabase_ThrowsArgumentException()
    {
        // Arrange
        _recommendationService
            .GetRecommendationsByContentfulReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns(new List<SqlRecommendationDto>());
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                new TestController(),
                SectionSlug,
                RecommendationSlug
            )
        );
        Assert.Equal("Could not find recommendation", exception.Message);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenCalled_ReturnsSelectSchoolsToUpdateStatusView()
    {
        // Arrange
        var controller = new TestController();
        controller.RouteData.Values["categorySlug"] = CategorySlug;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            controller,
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("GroupsSelectSchoolsToUpdateStatus", view.ViewName);
        var viewModel = Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(view.Model);
        Assert.Same(_section, viewModel.Section);
        Assert.Same(_section.CoreRecommendations.Single(), viewModel.RecommendationChunk);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenRecommendationSlugDiffersByCase_FindsRecommendation()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug.ToUpperInvariant()
        );

        // Assert
        var viewModel = Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Same(_section.CoreRecommendations.Single(), viewModel.RecommendationChunk);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenViewModelIsProvided_ReusesIt()
    {
        // Arrange
        var providedViewModel = new GroupsSelectSchoolsToUpdateStatusViewModel
        {
            SelectedSchoolsRefs = ["100011"],
        };
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug,
            providedViewModel
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(providedViewModel, view.Model);
        Assert.Equal(["100011"], providedViewModel.SelectedSchoolsRefs);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenSchoolsHaveRecommendationHistory_MapsHistoryToEstablishmentRecommendations()
    {
        // Arrange
        var history = new EstablishmentRecommendationHistoryEntity
        {
            Id = 99,
            DateCreated = new DateTime(2025, 3, 4, 10, 30, 0, DateTimeKind.Utc),
            NewStatus = RecommendationStatus.InProgress,
        };
        _groupService
            .GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRef(
                TrustEstablishmentId,
                RecommendationChunkId
            )
            .Returns(new EstablishmentHistories { (_oakfieldEntity, history) });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        var viewModel = Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        var establishmentRecommendation = Assert.Single(viewModel.EstablishmentsRecommendation!);
        Assert.Equal(11, establishmentRecommendation.EstablishmentId);
        Assert.Equal("Oakfield Primary", establishmentRecommendation.EstablishmentName);
        Assert.Equal("100011", establishmentRecommendation.EstablishmentRef);
        Assert.Equal(SectionId, establishmentRecommendation.SectionId);
        Assert.Equal(99, establishmentRecommendation.EstablishmentRecommendationHistoryId);
        Assert.Equal("4 Mar 2025", establishmentRecommendation.DateLastUpdated);
        Assert.Equal(RecommendationStatus.InProgress, establishmentRecommendation.Status);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenSchoolHasNoRecommendationHistory_ExcludesSchool()
    {
        // Arrange
        var history = new EstablishmentRecommendationHistoryEntity
        {
            Id = 99,
            NewStatus = RecommendationStatus.Complete,
        };
        _groupService
            .GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRef(
                TrustEstablishmentId,
                RecommendationChunkId
            )
            .Returns(
                new EstablishmentHistories { (_oakfieldEntity, history), (_riverbankEntity, null) }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        var viewModel = Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        var establishmentRecommendation = Assert.Single(viewModel.EstablishmentsRecommendation!);
        Assert.Equal(11, establishmentRecommendation.EstablishmentId);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenHistoryHasNoNewStatus_DefaultsStatusToNotStarted()
    {
        // Arrange
        var history = new EstablishmentRecommendationHistoryEntity { Id = 99, NewStatus = null };
        _groupService
            .GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRef(
                TrustEstablishmentId,
                RecommendationChunkId
            )
            .Returns(new EstablishmentHistories { (_oakfieldEntity, history) });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        var viewModel = Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        var establishmentRecommendation = Assert.Single(viewModel.EstablishmentsRecommendation!);
        Assert.Equal(RecommendationStatus.NotStarted, establishmentRecommendation.Status);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenSeveralRecommendationsMatch_UsesLatestRecommendationReference()
    {
        // Arrange
        _recommendationService
            .GetRecommendationsByContentfulReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([
                new SqlRecommendationDto { Id = 1, ContentfulSysId = "older-reference" },
                new SqlRecommendationDto { Id = 3, ContentfulSysId = "latest-reference" },
                new SqlRecommendationDto { Id = 2, ContentfulSysId = "middle-reference" },
            ]);
        var history = new EstablishmentRecommendationHistoryEntity { Id = 99 };
        _groupService
            .GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRef(
                TrustEstablishmentId,
                "latest-reference"
            )
            .Returns(new EstablishmentHistories { (_oakfieldEntity, history) });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        var viewModel = Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Single(viewModel.EstablishmentsRecommendation!);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToUpdateStatusViewModelAsync_WhenModelStateHasErrors_SetsErrorMessages()
    {
        // Arrange
        var controller = new TestController();
        controller.ModelState.AddModelError("SelectedSchoolsRefs", "Select at least one school");
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            controller,
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        var viewModel = Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal(["Select at least one school"], viewModel.ErrorMessages);
    }
}
