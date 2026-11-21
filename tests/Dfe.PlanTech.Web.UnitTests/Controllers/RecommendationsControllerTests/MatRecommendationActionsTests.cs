using Dfe.PlanTech.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.RecommendationsControllerTests;

public class MatRecommendationActionsTests : RecommendationsControllerTestBase
{
    [Fact]
    public async Task GetMatSingleRecommendation_WhenCalled_ReturnsViewBuilderResultForMatContext()
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
                CategoryLandingContext.MAT
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetMatSingleRecommendation(
            CategorySlug,
            SectionSlug,
            ChunkSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task GetMatSingleRecommendation_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.GetMatSingleRecommendation(categorySlug!, sectionSlug!, chunkSlug!)
        );
    }

    [Fact]
    public async Task PrintMatSingleRecommendation_WhenCalled_ReturnsViewBuilderResultForMatContext()
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
                CategoryLandingContext.MAT
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.PrintMatSingleRecommendation(
            CategorySlug,
            SectionSlug,
            ChunkSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task PrintMatSingleRecommendation_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.PrintMatSingleRecommendation(categorySlug!, sectionSlug!, chunkSlug!)
        );
    }

    [Fact]
    public async Task PrintMatAllRecommendations_WhenCalled_ReturnsViewBuilderResultForMatContext()
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
                CategoryLandingContext.MAT
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.PrintMatAllRecommendations(
            CategorySlug,
            SectionSlug,
            ChunkSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task PrintMatAllRecommendations_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.PrintMatAllRecommendations(categorySlug!, sectionSlug!, chunkSlug!)
        );
    }

    [Fact]
    public async Task ShareMatSingleRecommendation_WhenCalled_ReturnsViewBuilderResultForMatContext()
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
                CategoryLandingContext.MAT
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.ShareMatSingleRecommendation(
            CategorySlug,
            SectionSlug,
            ChunkSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task ShareMatSingleRecommendation_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.ShareMatSingleRecommendation(categorySlug!, sectionSlug!, chunkSlug!)
        );
    }

    [Fact]
    public async Task PostShareMatSingleRecommendation_WhenCalled_ReturnsViewBuilderResultForMatContext()
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
                CategoryLandingContext.MAT
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.PostShareMatSingleRecommendation(
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
    public async Task PostShareMatSingleRecommendation_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.PostShareMatSingleRecommendation(
                categorySlug!,
                sectionSlug!,
                chunkSlug!,
                ShareInputModel
            )
        );
    }
}
