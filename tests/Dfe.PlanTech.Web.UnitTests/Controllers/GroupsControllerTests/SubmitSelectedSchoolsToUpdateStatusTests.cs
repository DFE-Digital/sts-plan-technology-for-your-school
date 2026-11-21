using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class SubmitSelectedSchoolsToUpdateStatusTests : GroupsControllerTestBase
{
    private readonly GroupsSelectSchoolsToUpdateStatusViewModel _viewModel = new()
    {
        SelectedSchoolsRefs = ["100011", "100012"],
    };

    [Theory]
    [InlineData(null, RecommendationSlug, typeof(ArgumentNullException))]
    [InlineData(SectionSlug, null, typeof(ArgumentNullException))]
    [InlineData("", RecommendationSlug, typeof(ArgumentException))]
    [InlineData(SectionSlug, "   ", typeof(ArgumentException))]
    public async Task SubmitSelectedSchoolsToUpdateStatus_WhenASlugIsMissing_ThrowsArgumentException(
        string? sectionSlug,
        string? recommendationSlug,
        Type expectedExceptionType
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () =>
            systemUnderTest.SubmitSelectedSchoolsToUpdateStatus(
                _viewModel,
                sectionSlug!,
                recommendationSlug!
            )
        );
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToUpdateStatus_WhenCalled_ValidatesSelectionAgainstModelState()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitSelectedSchoolsToUpdateStatus(
            _viewModel,
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        await _groupSelectSchoolsToUpdateStatusValidator
            .Received(1)
            .ValidateSelectionAsync(_viewModel, systemUnderTest.ModelState);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToUpdateStatus_WhenModelStateIsValid_ReturnsSelectStatusViewForSelectedSchools()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder
            .RouteToSelectStatusToUpdateViewModelAsync(
                systemUnderTest,
                SectionSlug,
                RecommendationSlug,
                Arg.Is<GroupsSelectStatusToUpdateViewModel>(statusViewModel =>
                    statusViewModel.SelectedStatusKey == RecommendationStatus.NotStarted
                    && ReferenceEquals(
                        statusViewModel.SelectedSchoolsRefs,
                        _viewModel.SelectedSchoolsRefs
                    )
                )
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.SubmitSelectedSchoolsToUpdateStatus(
            _viewModel,
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToUpdateStatus_WhenModelStateIsInvalid_ReturnsSelectSchoolsView()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.ModelState.AddModelError("SelectedSchoolsRefs", "Select a school");
        _groupsViewBuilder
            .RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                systemUnderTest,
                SectionSlug,
                RecommendationSlug,
                _viewModel
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.SubmitSelectedSchoolsToUpdateStatus(
            _viewModel,
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
        await _groupsViewBuilder
            .DidNotReceiveWithAnyArgs()
            .RouteToSelectStatusToUpdateViewModelAsync(default!, default!, default!, default);
    }
}
