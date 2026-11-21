using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class GetMatRecommendationsLandingTests : GroupsControllerTestBase
{
    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public async Task GetMatRecommendationsLanding_WhenCategorySlugIsMissing_ThrowsArgumentException(
        string? categorySlug,
        Type expectedExceptionType
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () =>
            systemUnderTest.GetMatRecommendationsLanding(categorySlug!)
        );
    }

    [Fact]
    public async Task GetMatRecommendationsLanding_WhenCalled_ReturnsPagesViewBuilderResult()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        _pagesViewBuilder
            .RouteToMatCategoryLandingPageAsync(systemUnderTest, CategorySlug)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetMatRecommendationsLanding(CategorySlug);

        // Assert
        Assert.Same(expectedResult, result);
    }
}
