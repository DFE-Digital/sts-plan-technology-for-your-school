using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using EstablishmentHistories = System.Collections.Generic.List<(
    Dfe.PlanTech.Data.Sql.Entities.EstablishmentEntity establishment,
    Dfe.PlanTech.Data.Sql.Entities.EstablishmentRecommendationHistoryEntity? recommendationHistory
)>;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class RouteToSelectStatusToUpdateViewModelAsyncTests : GroupsViewBuilderTestBase
{
    private const int TrustEstablishmentId = 500;

    public static IEnumerable<object?[]> NoSelectedSchoolRefs =>
        [
            [null],
            [new List<string>()],
        ];

    private readonly GroupsSelectStatusToUpdateViewModel _viewModel = new()
    {
        SelectedStatusKey = RecommendationStatus.NotStarted,
        SelectedSchoolsRefs = ["100011"],
    };

    public RouteToSelectStatusToUpdateViewModelAsyncTests()
    {
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(TrustEstablishmentId).Build());
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _oakfieldPrimary, _riverbankPrimary }
            );
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        _recommendationService
            .GetRecommendationsByContentfulReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([
                new SqlRecommendationDto { Id = 1, ContentfulSysId = RecommendationChunkId },
            ]);
        _groupService
            .GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRef(
                TrustEstablishmentId,
                RecommendationChunkId
            )
            .Returns(new EstablishmentHistories());
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns(
                new List<SqlEstablishmentDto>
                {
                    new SqlEstablishmentDtoBuilder()
                        .WithId(11)
                        .WithEstablishmentRef("100011")
                        .WithOrgName("Oakfield Primary")
                        .Build(),
                }
            );
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenViewModelIsNull_ReturnsSelectSchoolsToUpdateStatusView()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("GroupsSelectSchoolsToUpdateStatus", view.ViewName);
        Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(view.Model);
    }

    [Theory]
    [MemberData(nameof(NoSelectedSchoolRefs))]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenNoSchoolsAreSelected_ReturnsSelectSchoolsToUpdateStatusView(
        List<string>? selectedSchoolRefs
    )
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = selectedSchoolRefs;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug,
            _viewModel
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("GroupsSelectSchoolsToUpdateStatus", view.ViewName);
        Assert.IsType<GroupsSelectSchoolsToUpdateStatusViewModel>(view.Model);
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSectionIsMissing_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
                new TestController(),
                SectionSlug,
                RecommendationSlug,
                _viewModel
            )
        );
        Assert.Contains(SectionSlug, exception.Message);
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenRecommendationSlugIsNotInSection_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
                new TestController(),
                SectionSlug,
                "unknown-recommendation-slug",
                _viewModel
            )
        );
        Assert.Contains("unknown-recommendation-slug", exception.Message);
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSelectedSchoolIsNotInGroup_ThrowsInvalidDataException()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["100011", "999999"];
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
                new TestController(),
                SectionSlug,
                RecommendationSlug,
                _viewModel
            )
        );
        await _currentUserProvider
            .DidNotReceive()
            .SetSelectedSchoolsAsync(Arg.Any<IEnumerable<SelectedEstablishmentDataModel>>());
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSchoolsAreSelected_ReturnsSelectStatusView()
    {
        // Arrange
        var controller = new TestController();
        controller.RouteData.Values["categorySlug"] = CategorySlug;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            controller,
            SectionSlug,
            RecommendationSlug,
            _viewModel
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("GroupsSelectStatusToUpdate", view.ViewName);
        var viewModel = Assert.IsType<GroupsSelectStatusToUpdateViewModel>(view.Model);
        Assert.Same(_section, viewModel.Section);
        Assert.Same(_section.CoreRecommendations.Single(), viewModel.CurrentChunk);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal(
            "Select status for recommendation",
            controller.ViewData[StatePassingMechanismConstants.Title]
        );
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSchoolsAreSelected_SetsSelectedSchoolNames()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug,
            _viewModel
        );

        // Assert
        Assert.Equal(["Oakfield Primary"], _viewModel.SelectedSchoolNames);
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSchoolsAreSelected_SetsStatusOptionsFromEveryRecommendationStatus()
    {
        // Arrange
        var expectedStatusOptions = new Dictionary<RecommendationStatus, string>
        {
            [RecommendationStatus.NotStarted] = "Not\u00A0started",
            [RecommendationStatus.InProgress] = "In\u00A0progress",
            [RecommendationStatus.Complete] = "Complete",
        };
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug,
            _viewModel
        );

        // Assert
        Assert.Equal(expectedStatusOptions, _viewModel.StatusOptions);
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSchoolsAreSelected_SetsSelectedSchoolsFromGroupSchools()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["100012", "100011"];
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug,
            _viewModel
        );

        // Assert
        await _currentUserProvider
            .Received(1)
            .SetSelectedSchoolsAsync(
                Arg.Is<IEnumerable<SelectedEstablishmentDataModel>>(schools =>
                    schools.Select(school => school.EstablishmentId).SequenceEqual(new[] { 12, 11 })
                    && schools
                        .Select(school => school.Urn)
                        .SequenceEqual(new[] { "100012", "100011" })
                    && schools
                        .Select(school => school.Name)
                        .SequenceEqual(new[] { "Riverbank Primary", "Oakfield Primary" })
                )
            );
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSelectedRefsHaveDuplicatesAndBlanks_SetsEachSchoolOnce()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["100011", " ", "100011"];
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug,
            _viewModel
        );

        // Assert
        await _currentUserProvider
            .Received(1)
            .SetSelectedSchoolsAsync(
                Arg.Is<IEnumerable<SelectedEstablishmentDataModel>>(schools =>
                    schools.Select(school => school.Urn).SequenceEqual(new[] { "100011" })
                )
            );
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSelectedSchoolHasInProgressSubmission_SetsSubmissionInaccessible()
    {
        // Arrange
        _currentUserProvider
            .GetSelectedSchools()
            .Returns(new List<SelectedEstablishmentDataModel> { _oakfieldPrimary });
        _submissionService
            .GetLatestSubmissionResponsesModel(11, _section, (SubmissionStatus?)null)
            .Returns(new SubmissionResponsesModel(1, []) { Status = SubmissionStatus.InProgress });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug,
            _viewModel
        );

        // Assert
        await _submissionService.Received(1).SetSubmissionInaccessibleAsync(11, SectionId);
    }

    [Fact]
    public async Task RouteToSelectStatusToUpdateViewModelAsync_WhenSelectedSchoolHasNoInProgressSubmission_DoesNotSetSubmissionInaccessible()
    {
        // Arrange
        _currentUserProvider
            .GetSelectedSchools()
            .Returns(new List<SelectedEstablishmentDataModel> { _oakfieldPrimary });
        _submissionService
            .GetLatestSubmissionResponsesModel(11, _section, (SubmissionStatus?)null)
            .Returns(
                new SubmissionResponsesModel(1, []) { Status = SubmissionStatus.CompleteReviewed }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToSelectStatusToUpdateViewModelAsync(
            new TestController(),
            SectionSlug,
            RecommendationSlug,
            _viewModel
        );

        // Assert
        await _submissionService
            .DidNotReceiveWithAnyArgs()
            .SetSubmissionInaccessibleAsync(default, default!);
    }
}
