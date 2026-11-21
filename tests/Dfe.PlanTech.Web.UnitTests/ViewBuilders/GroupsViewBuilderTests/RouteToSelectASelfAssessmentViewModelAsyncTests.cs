using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class RouteToSelectASelfAssessmentViewModelAsyncTests : GroupsViewBuilderTestBase
{
    private const string GroupName = "Springfield Academy Trust";

    private readonly QuestionnaireCategoryEntry _cyberSecurityCategory = new()
    {
        Sys = new SystemDetails("category-cyber"),
        Header = new ComponentHeaderEntry { Text = "Cyber security" },
        LandingPage = new PageEntry { Slug = "cyber-security-standard" },
        Sections =
        [
            new QuestionnaireSectionEntry
            {
                Sys = new SystemDetails("section-cyber-1"),
                Name = "Cyber security processes",
                InterstitialPage = new PageEntry { Slug = "cyber-security-processes" },
            },
        ],
    };

    public RouteToSelectASelfAssessmentViewModelAsyncTests()
    {
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(GroupName).Build());
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns(new List<QuestionnaireCategoryEntry> { _cyberSecurityCategory });
        _contentfulService.GetPageBySlugAsync("home").Returns(new PageEntry());
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _oakfieldPrimary, _riverbankPrimary }
            );
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenNoCategoriesExist_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService.GetAllCategoriesAsync().Returns(new List<QuestionnaireCategoryEntry>());
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(new TestController())
        );
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenCalled_ReturnsSelectSelfAssessmentView()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("GroupsSelectSelfAssessment", view.ViewName);
        Assert.IsType<GroupSelectAssessmentViewModel>(view.Model);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenCalled_SetsGroupNameFromUserEstablishment()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        var viewModel = Assert.IsType<GroupSelectAssessmentViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal(GroupName, viewModel.GroupName);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenOrganisationHasNoName_UsesDefaultGroupName()
    {
        // Arrange
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(null).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        var viewModel = Assert.IsType<GroupSelectAssessmentViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal("Your organisation", viewModel.GroupName);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenCalled_MapsCategoriesAndSections()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        var viewModel = Assert.IsType<GroupSelectAssessmentViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        var category = Assert.Single(viewModel.Categories);
        Assert.Equal("Cyber security", category.CategoryName);
        var section = Assert.Single(category.Sections);
        Assert.Equal("Cyber security processes", section.SectionName);
        Assert.Equal("cyber-security-standard", section.CategorySlug);
        Assert.Equal("cyber-security-processes", section.SectionSlug);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenCategoryHasNoHeaderOrLandingPage_UsesEmptyNameAndNoSlug()
    {
        // Arrange
        _cyberSecurityCategory.Header = null!;
        _cyberSecurityCategory.LandingPage = null;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        var viewModel = Assert.IsType<GroupSelectAssessmentViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        var category = Assert.Single(viewModel.Categories);
        Assert.Equal(string.Empty, category.CategoryName);
        Assert.Null(Assert.Single(category.Sections).CategorySlug);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenHomePageOrdersCategories_ListsHomePageCategoriesBeforeTheRest()
    {
        // Arrange
        var governanceCategory = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-governance"),
            Header = new ComponentHeaderEntry { Text = "Leadership and governance" },
        };
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns(
                new List<QuestionnaireCategoryEntry> { _cyberSecurityCategory, governanceCategory }
            );
        _contentfulService
            .GetPageBySlugAsync("home")
            .Returns(new PageEntry { Content = [governanceCategory] });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        var viewModel = Assert.IsType<GroupSelectAssessmentViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal(
            ["Leadership and governance", "Cyber security"],
            viewModel.Categories.Select(category => category.CategoryName)
        );
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenHomePageIsMissing_UsesAllCategories()
    {
        // Arrange
        _contentfulService.GetPageBySlugAsync("home").Returns((PageEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        var viewModel = Assert.IsType<GroupSelectAssessmentViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Single(viewModel.Categories);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenNoSchoolHasCompletedSection_CountsEverySchoolAsUncompleted()
    {
        // Arrange
        ArrangeCompletedSubmissions();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        Assert.Equal(2, GetOnlySection(result).UncompletedGroupSubmissions);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenOneSchoolHasCompletedSection_CountsRemainingSchools()
    {
        // Arrange
        ArrangeCompletedSubmissions(
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "section-cyber-1" }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        Assert.Equal(1, GetOnlySection(result).UncompletedGroupSubmissions);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenEverySchoolHasCompletedSection_CountsNoUncompletedSchools()
    {
        // Arrange
        ArrangeCompletedSubmissions(
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "section-cyber-1" },
            new SqlSubmissionDto { EstablishmentId = 12, SectionId = "section-cyber-1" }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        Assert.Equal(0, GetOnlySection(result).UncompletedGroupSubmissions);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenSchoolHasDuplicateCompletedSubmissions_CountsSchoolOnce()
    {
        // Arrange
        ArrangeCompletedSubmissions(
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "section-cyber-1" },
            new SqlSubmissionDto { EstablishmentId = 11, SectionId = "section-cyber-1" }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        Assert.Equal(1, GetOnlySection(result).UncompletedGroupSubmissions);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenCompletedSubmissionIsForSchoolOutsideGroup_IgnoresIt()
    {
        // Arrange
        ArrangeCompletedSubmissions(
            new SqlSubmissionDto { EstablishmentId = 999, SectionId = "section-cyber-1" }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        Assert.Equal(2, GetOnlySection(result).UncompletedGroupSubmissions);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenCompletedSubmissionsAreUnavailable_CountsEverySchoolAsUncompleted()
    {
        // Arrange
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns((List<SqlSubmissionDto>)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        Assert.Equal(2, GetOnlySection(result).UncompletedGroupSubmissions);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_WhenGroupHasNoSchools_SkipsSubmissionLookupAndCountsNoUncompletedSchools()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(new List<SelectedEstablishmentDataModel>());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectASelfAssessmentViewModelAsync(
            new TestController()
        );

        // Assert
        Assert.Equal(0, GetOnlySection(result).UncompletedGroupSubmissions);
        await _groupService
            .DidNotReceiveWithAnyArgs()
            .GetGroupCompletedSubmissionsBySections(default!);
    }

    private void ArrangeCompletedSubmissions(params SqlSubmissionDto[] completedSubmissions) =>
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(completedSubmissions.ToList());

    private static GroupSelectAssessmentSectionViewModel GetOnlySection(IActionResult result)
    {
        var viewModel = Assert.IsType<GroupSelectAssessmentViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        return Assert.Single(Assert.Single(viewModel.Categories).Sections);
    }
}
