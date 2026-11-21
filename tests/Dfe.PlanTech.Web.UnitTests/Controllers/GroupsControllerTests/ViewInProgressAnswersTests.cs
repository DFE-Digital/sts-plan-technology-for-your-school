using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class ViewInProgressAnswersTests : GroupsControllerTestBase
{
    private const string SchoolUrn = "100011";

    [Theory]
    [InlineData(null, SectionSlug, SchoolUrn, typeof(ArgumentNullException))]
    [InlineData(CategorySlug, null, SchoolUrn, typeof(ArgumentNullException))]
    [InlineData(CategorySlug, SectionSlug, null, typeof(ArgumentNullException))]
    [InlineData("", SectionSlug, SchoolUrn, typeof(ArgumentException))]
    [InlineData(CategorySlug, "   ", SchoolUrn, typeof(ArgumentException))]
    [InlineData(CategorySlug, SectionSlug, "", typeof(ArgumentException))]
    public async Task ViewInProgressAnswers_WhenAnArgumentIsMissing_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? schoolUrn,
        Type expectedExceptionType
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () =>
            systemUnderTest.ViewInProgressAnswers(categorySlug!, sectionSlug!, schoolUrn!)
        );
    }

    [Fact]
    public async Task ViewInProgressAnswers_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder
            .RouteToViewInProgressAnswers(systemUnderTest, CategorySlug, SectionSlug, SchoolUrn)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.ViewInProgressAnswers(
            CategorySlug,
            SectionSlug,
            SchoolUrn
        );

        // Assert
        Assert.Same(expectedResult, result);
    }
}
