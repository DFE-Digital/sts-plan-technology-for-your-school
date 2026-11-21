using Dfe.PlanTech.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.RecommendationsControllerTests;

public class SchoolRecommendationActionsTests : RecommendationsControllerTestBase
{
    [Fact]
    public async Task GetSingleRecommendation_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _recommendationsViewBuilder
            .RouteToSingleRecommendation(
                systemUnderTest,
                CategorySlug,
                SectionSlug,
                ChunkSlug,
                CategoryLandingContext.School
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetSingleRecommendation(
            CategorySlug,
            SectionSlug,
            ChunkSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task GetSingleRecommendation_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.GetSingleRecommendation(categorySlug!, sectionSlug!, chunkSlug!)
        );
    }

    [Fact]
    public async Task PrintSingleRecommendation_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _recommendationsViewBuilder
            .RouteToPrintSingle(
                systemUnderTest,
                CategorySlug,
                SectionSlug,
                ChunkSlug,
                CategoryLandingContext.School
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.PrintSingleRecommendation(
            CategorySlug,
            SectionSlug,
            ChunkSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task PrintSingleRecommendation_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.PrintSingleRecommendation(categorySlug!, sectionSlug!, chunkSlug!)
        );
    }

    [Fact]
    public async Task PrintAllRecommendations_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _recommendationsViewBuilder
            .RouteToPrintAll(
                systemUnderTest,
                CategorySlug,
                SectionSlug,
                ChunkSlug,
                CategoryLandingContext.School
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.PrintAllRecommendations(
            CategorySlug,
            SectionSlug,
            ChunkSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task PrintAllRecommendations_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.PrintAllRecommendations(categorySlug!, sectionSlug!, chunkSlug!)
        );
    }

    [Fact]
    public async Task ShareSingleRecommendation_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _recommendationsViewBuilder
            .RouteToShareRecommendationAsync(
                systemUnderTest,
                CategorySlug,
                SectionSlug,
                ChunkSlug,
                null,
                CategoryLandingContext.School
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.ShareSingleRecommendation(
            CategorySlug,
            SectionSlug,
            ChunkSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task ShareSingleRecommendation_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.ShareSingleRecommendation(categorySlug!, sectionSlug!, chunkSlug!)
        );
    }

    [Fact]
    public async Task PostShareSingleRecommendation_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _recommendationsViewBuilder
            .RouteToShareRecommendationAsync(
                systemUnderTest,
                CategorySlug,
                SectionSlug,
                ChunkSlug,
                ShareInputModel,
                CategoryLandingContext.School
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.PostShareSingleRecommendation(
            CategorySlug,
            SectionSlug,
            ChunkSlug,
            ShareInputModel
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task PostShareSingleRecommendation_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.PostShareSingleRecommendation(
                categorySlug!,
                sectionSlug!,
                chunkSlug!,
                ShareInputModel
            )
        );
    }
}
