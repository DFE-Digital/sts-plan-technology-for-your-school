using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class GetSelectSchoolsToAssessViewTests : GroupsControllerTestBase
{
    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public async Task GetSelectSchoolsToAssessView_WhenSectionSlugIsMissing_ThrowsArgumentException(
        string? sectionSlug,
        Type expectedExceptionType
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () =>
            systemUnderTest.GetSelectSchoolsToAssessView(sectionSlug!)
        );
    }

    [Fact]
    public async Task GetSelectSchoolsToAssessView_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder
            .RouteToSelectSchoolsToAssessViewModelAsync(systemUnderTest, SectionSlug)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetSelectSchoolsToAssessView(SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
    }
}
