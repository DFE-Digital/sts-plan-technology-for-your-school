using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class UpdateSelectedSchoolsStatusTests : GroupsControllerTestBase
{
    private readonly GroupRecommendationInputViewModel _inputViewModel = new()
    {
        SelectedSchoolsRefs = ["100011", "100012"],
        SelectedStatus = "Complete",
    };

    [Theory]
    [InlineData(null, RecommendationSlug, typeof(ArgumentNullException))]
    [InlineData(SectionSlug, null, typeof(ArgumentNullException))]
    [InlineData("", RecommendationSlug, typeof(ArgumentException))]
    [InlineData(SectionSlug, "   ", typeof(ArgumentException))]
    public async Task UpdateSelectedSchoolsStatus_WhenASlugIsMissing_ThrowsArgumentException(
        string? sectionSlug,
        string? recommendationSlug,
        Type expectedExceptionType
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () =>
            systemUnderTest.UpdateSelectedSchoolsStatus(
                _inputViewModel,
                sectionSlug!,
                recommendationSlug!
            )
        );
    }

    [Fact]
    public async Task UpdateSelectedSchoolsStatus_WhenModelStateIsValid_ReturnsUpdateResult()
    {
        // Arrange
        var expectedResult = new RedirectResult("/next-page");
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder
            .UpdateSchoolsRecommendationStatusAsync(
                systemUnderTest,
                SectionSlug,
                RecommendationSlug,
                _inputViewModel
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.UpdateSelectedSchoolsStatus(
            _inputViewModel,
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
        await _groupsViewBuilder
            .DidNotReceiveWithAnyArgs()
            .RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                default!,
                default!,
                default!,
                default
            );
    }

    [Fact]
    public async Task UpdateSelectedSchoolsStatus_WhenModelStateIsInvalid_ReturnsSelectSchoolsView()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.ModelState.AddModelError("SelectedStatus", "Select a status");
        _groupsViewBuilder
            .RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                systemUnderTest,
                SectionSlug,
                RecommendationSlug
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.UpdateSelectedSchoolsStatus(
            _inputViewModel,
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
        await _groupsViewBuilder
            .DidNotReceiveWithAnyArgs()
            .UpdateSchoolsRecommendationStatusAsync(default!, default!, default!, default!);
    }
}
