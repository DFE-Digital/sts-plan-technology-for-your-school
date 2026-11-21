using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class SubmitSelectedSchoolsToAssessTests : GroupsControllerTestBase
{
    private readonly GroupsSelectSchoolsToAssessViewModel _viewModel = new()
    {
        SelectedSchoolsRefs = ["100011", "100012"],
    };

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public async Task SubmitSelectedSchoolsToAssess_WhenSectionSlugIsMissing_ThrowsArgumentException(
        string? sectionSlug,
        Type expectedExceptionType
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () =>
            systemUnderTest.SubmitSelectedSchoolsToAssess(_viewModel, sectionSlug!)
        );
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssess_WhenCalled_ValidatesSelectionAgainstModelState()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitSelectedSchoolsToAssess(_viewModel, SectionSlug);

        // Assert
        await _groupSelectSchoolsToAssessValidator
            .Received(1)
            .ValidateSelectionAsync(_viewModel, systemUnderTest.ModelState);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssess_WhenModelStateIsValid_ReturnsSubmitResult()
    {
        // Arrange
        var expectedResult = new RedirectResult("/next-page");
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder
            .SubmitSelectedSchoolsToAssessAndRedirect(systemUnderTest, SectionSlug, _viewModel)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.SubmitSelectedSchoolsToAssess(_viewModel, SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
        await _groupsViewBuilder
            .DidNotReceiveWithAnyArgs()
            .RouteToSelectSchoolsToAssessViewModelAsync(default!, default!, default);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssess_WhenModelStateIsInvalid_ReturnsSelectSchoolsView()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.ModelState.AddModelError("SelectedSchoolsRefs", "Select a school");
        _groupsViewBuilder
            .RouteToSelectSchoolsToAssessViewModelAsync(systemUnderTest, SectionSlug, _viewModel)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.SubmitSelectedSchoolsToAssess(_viewModel, SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
        await _groupsViewBuilder
            .DidNotReceiveWithAnyArgs()
            .SubmitSelectedSchoolsToAssessAndRedirect(default!, default!, default!);
    }
}
