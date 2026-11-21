using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class GetSelectSchoolsToUpdateViewTests : GroupsControllerTestBase
{
    [Theory]
    [InlineData(null, RecommendationSlug, typeof(ArgumentNullException))]
    [InlineData(SectionSlug, null, typeof(ArgumentNullException))]
    [InlineData("", RecommendationSlug, typeof(ArgumentException))]
    [InlineData(SectionSlug, "   ", typeof(ArgumentException))]
    public async Task GetSelectSchoolsToUpdateView_WhenASlugIsMissing_ThrowsArgumentException(
        string? sectionSlug,
        string? recommendationSlug,
        Type expectedExceptionType
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () =>
            systemUnderTest.GetSelectSchoolsToUpdateView(sectionSlug!, recommendationSlug!)
        );
    }

    [Fact]
    public async Task GetSelectSchoolsToUpdateView_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder
            .RouteToSelectSchoolsToUpdateStatusViewModelAsync(
                systemUnderTest,
                SectionSlug,
                RecommendationSlug
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetSelectSchoolsToUpdateView(
            SectionSlug,
            RecommendationSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }
}
