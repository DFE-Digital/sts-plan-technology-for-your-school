using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class RouteToSelectSchoolsToAssessViewModelAsyncTests : GroupsViewBuilderTestBase
{
    private readonly List<SelectedEstablishmentDataModel> _groupSchools;

    private readonly SubmissionInformationModel _oakfieldSubmission = new()
    {
        EstablishmentId = 11,
        EstablishmentName = "Oakfield Primary",
        EstablishmentRef = "100011",
        SectionId = SectionId,
        Status = SubmissionStatus.InProgress,
    };

    private readonly SubmissionInformationModel _riverbankSubmission = new()
    {
        EstablishmentId = 12,
        EstablishmentName = "Riverbank Primary",
        EstablishmentRef = "100012",
        SectionId = SectionId,
        Status = SubmissionStatus.CompleteReviewed,
    };

    public RouteToSelectSchoolsToAssessViewModelAsyncTests()
    {
        _groupSchools = [_oakfieldPrimary, _riverbankPrimary];
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        _currentUserProvider.GetGroupSchoolsAsync().Returns(_groupSchools);
        _groupService
            .GetGroupSubmissionInformationForSection(_groupSchools, SectionId)
            .Returns(
                new List<SubmissionInformationModel> { _oakfieldSubmission, _riverbankSubmission }
            );
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_WhenCalled_ClearsSelectedSchools()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToSelectSchoolsToAssessViewModelAsync(
            new TestController(),
            SectionSlug
        );

        // Assert
        _currentUserProvider.Received(1).ClearSelectedSchools();
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_WhenSectionIsMissing_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelectSchoolsToAssessViewModelAsync(
                new TestController(),
                SectionSlug
            )
        );
        Assert.Contains(SectionSlug, exception.Message);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_WhenEverySchoolIsCompleteAndReviewed_RedirectsToSelectSelfAssessment()
    {
        // Arrange
        _groupService
            .GetGroupSubmissionInformationForSection(_groupSchools, SectionId)
            .Returns(new List<SubmissionInformationModel> { _riverbankSubmission });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToAssessViewModelAsync(
            new TestController(),
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal(GroupsController.GetSelectASelfAssessmentAction, redirect.RouteName);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_WhenNoSchoolHasSubmissionInformation_RedirectsToSelectSelfAssessment()
    {
        // Arrange
        _groupService
            .GetGroupSubmissionInformationForSection(_groupSchools, SectionId)
            .Returns(new List<SubmissionInformationModel>());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToAssessViewModelAsync(
            new TestController(),
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal(GroupsController.GetSelectASelfAssessmentAction, redirect.RouteName);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_WhenSomeSchoolsAreEligible_ReturnsSelectSchoolsToAssessView()
    {
        // Arrange
        var controller = new TestController();
        controller.RouteData.Values["categorySlug"] = CategorySlug;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToAssessViewModelAsync(
            controller,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("GroupSelectSchoolsToAssess", view.ViewName);
        var viewModel = Assert.IsType<GroupsSelectSchoolsToAssessViewModel>(view.Model);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Same(_section, viewModel.Section);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_WhenSomeSchoolsAreCompleteAndReviewed_ListsOnlyEligibleSchools()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToAssessViewModelAsync(
            new TestController(),
            SectionSlug
        );

        // Assert
        var viewModel = Assert.IsType<GroupsSelectSchoolsToAssessViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Same(_oakfieldSubmission, Assert.Single(viewModel.SchoolSubmissionInfo!));
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_WhenViewModelIsProvided_ReusesIt()
    {
        // Arrange
        var providedViewModel = new GroupsSelectSchoolsToAssessViewModel
        {
            SelectedSchoolsRefs = ["100011"],
        };
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToAssessViewModelAsync(
            new TestController(),
            SectionSlug,
            providedViewModel
        );

        // Assert
        Assert.Same(providedViewModel, Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(["100011"], providedViewModel.SelectedSchoolsRefs);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_WhenModelStateHasErrors_SetsErrorMessages()
    {
        // Arrange
        var controller = new TestController();
        controller.ModelState.AddModelError("SelectedSchoolsRefs", "Select at least one school");
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelectSchoolsToAssessViewModelAsync(
            controller,
            SectionSlug
        );

        // Assert
        var viewModel = Assert.IsType<GroupsSelectSchoolsToAssessViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal(["Select at least one school"], viewModel.ErrorMessages);
    }
}
