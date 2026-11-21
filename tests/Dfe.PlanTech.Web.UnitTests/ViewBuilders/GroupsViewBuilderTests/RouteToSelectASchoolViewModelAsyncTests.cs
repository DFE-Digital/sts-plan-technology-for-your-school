using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class RouteToSelectASchoolViewModelAsyncTests : GroupsViewBuilderTestBase
{
    private const string GroupName = "Springfield Academy Trust";
    private const string ContactHref = "/contact-us";

    public RouteToSelectASchoolViewModelAsyncTests()
    {
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(GroupName).Build());
        _contentfulService
            .GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            .Returns(new PageEntry());
        _contentfulService.GetAllSectionsAsync().Returns(new List<QuestionnaireSectionEntry>());
        _contentfulService
            .GetLinkByIdAsync(ContactLinkId)
            .Returns(new NavigationLinkEntry { Href = ContactHref });
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(new List<SelectedEstablishmentDataModel>());
        _recommendationService
            .GetInProgressOrCompletedRecommendationCounts(Arg.Any<IEnumerable<int>>())
            .Returns(new Dictionary<int, int>());
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenPageIsMissing_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            .Returns((PageEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController())
        );
        Assert.Contains(UrlConstants.GroupsSelectionPageSlug, exception.Message);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenCalled_ReturnsSelectSchoolView()
    {
        // Arrange
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(controller);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("GroupsSelectSchool", view.ViewName);
        Assert.IsType<GroupsSelectorViewModel>(view.Model);
        Assert.Equal("Select a school", controller.ViewData[StatePassingMechanismConstants.Title]);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenCalled_SetsGroupNameFromUserEstablishment()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal(GroupName, viewModel.GroupName);
        Assert.Equal(GroupName, viewModel.Title.Text);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenOrganisationHasNoName_UsesDefaultGroupName()
    {
        // Arrange
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(null).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal("Your organisation", viewModel.GroupName);
        Assert.Equal("Your organisation", viewModel.Title.Text);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenPageHasContent_SetsPageContent()
    {
        // Arrange
        var beforeTitleEntry = new MissingComponentEntry();
        var contentEntry = new MissingComponentEntry();
        _contentfulService
            .GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            .Returns(
                new PageEntry { BeforeTitleContent = [beforeTitleEntry], Content = [contentEntry] }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Same(beforeTitleEntry, Assert.Single(viewModel.BeforeTitleContent));
        Assert.Same(contentEntry, Assert.Single(viewModel.Content));
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenPageHasNoContent_SetsEmptyContent()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Empty(viewModel.Content);
        Assert.Empty(viewModel.BeforeTitleContent);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenSectionsHaveRecommendations_SetsTotalRecommendations()
    {
        // Arrange
        var firstSection = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("section-1"),
            CoreRecommendations = [new RecommendationChunkEntry(), new RecommendationChunkEntry()],
        };
        var secondSection = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("section-2"),
            CoreRecommendations =
            [
                new RecommendationChunkEntry(),
                new RecommendationChunkEntry(),
                new RecommendationChunkEntry(),
            ],
        };
        _contentfulService
            .GetAllSectionsAsync()
            .Returns(new List<QuestionnaireSectionEntry> { firstSection, secondSection });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal("5", viewModel.TotalRecommendations);
        Assert.Null(viewModel.ProgressRetrievalErrorMessage);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenGroupHasSchools_SetsSchoolsAndRecommendationCounts()
    {
        // Arrange
        var groupSchools = new List<SelectedEstablishmentDataModel>
        {
            _oakfieldPrimary,
            _riverbankPrimary,
        };
        int[] expectedEstablishmentIds = [11, 12];
        var expectedCounts = new Dictionary<int, int> { [11] = 3, [12] = 1 };
        _currentUserProvider.GetGroupSchoolsAsync().Returns(groupSchools);
        _recommendationService
            .GetInProgressOrCompletedRecommendationCounts(
                Arg.Is<IEnumerable<int>>(ids => ids.SequenceEqual(expectedEstablishmentIds))
            )
            .Returns(expectedCounts);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Same(groupSchools, viewModel.GroupEstablishments);
        Assert.Same(expectedCounts, viewModel.InProgressOrCompletedRecommendationCounts);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenContactLinkExists_SetsContactLinkHref()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal(ContactHref, viewModel.ContactLinkHref);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenContactLinkIsMissing_SetsNullContactLinkHref()
    {
        // Arrange
        _contentfulService.GetLinkByIdAsync(ContactLinkId).Returns((NavigationLinkEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Null(viewModel.ContactLinkHref);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenGroupHasNoSchools_HidesSelectSelfAssessmentAndSkipsSubmissionLookup()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.False(viewModel.ShowSelectSelfAssessmentToSubmit);
        await _groupService
            .DidNotReceiveWithAnyArgs()
            .GetGroupCompletedSubmissionsBySections(default!);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenSchoolHasNotCompletedEverySection_ShowsSelectSelfAssessment()
    {
        // Arrange
        ArrangeGroupWithTwoSchoolsAndOneSection(
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "section-1" }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.True(viewModel.ShowSelectSelfAssessmentToSubmit);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenEverySchoolHasCompletedEverySection_HidesSelectSelfAssessment()
    {
        // Arrange
        ArrangeGroupWithTwoSchoolsAndOneSection(
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "section-1" },
            new SqlSubmissionDto { EstablishmentId = 12, SectionId = "section-1" }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.False(viewModel.ShowSelectSelfAssessmentToSubmit);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenSchoolHasDuplicateCompletedSubmissions_CountsThemOnce()
    {
        // Arrange
        ArrangeGroupWithTwoSchoolsAndOneSection(
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "section-1" },
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "section-1" }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.True(viewModel.ShowSelectSelfAssessmentToSubmit);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenCompletedSubmissionsAreForOtherSections_IgnoresThem()
    {
        // Arrange
        ArrangeGroupWithTwoSchoolsAndOneSection(
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "other-section" },
            new SqlSubmissionDto { EstablishmentId = 12, SectionId = "other-section" }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.True(viewModel.ShowSelectSelfAssessmentToSubmit);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_WhenCompletedSubmissionsAreUnavailable_ShowsSelectSelfAssessment()
    {
        // Arrange
        ArrangeGroupWithTwoSchoolsAndOneSection();
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns((List<SqlSubmissionDto>)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASchoolViewModelAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<GroupsSelectorViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.True(viewModel.ShowSelectSelfAssessmentToSubmit);
    }

    private void ArrangeGroupWithTwoSchoolsAndOneSection(
        params SqlSubmissionDto[] completedSubmissions
    )
    {
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _oakfieldPrimary, _riverbankPrimary }
            );
        _contentfulService
            .GetAllSectionsAsync()
            .Returns(
                new List<QuestionnaireSectionEntry>
                {
                    new() { Sys = new SystemDetails("section-1") },
                }
            );
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(completedSubmissions.ToList());
    }
}
