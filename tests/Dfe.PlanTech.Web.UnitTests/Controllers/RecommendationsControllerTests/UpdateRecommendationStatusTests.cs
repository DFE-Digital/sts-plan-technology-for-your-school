using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.RecommendationsControllerTests;

public class UpdateRecommendationStatusTests : RecommendationsControllerTestBase
{
    [Fact]
    public async Task UpdateRecommendationStatus_WhenModelStateIsValid_ReturnsViewBuilderUpdateResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var inputModel = new SingleRecommendationInputViewModel { SelectedStatus = "Complete" };
        var expectedResult = new OkResult();
        _recommendationsViewBuilder
            .UpdateRecommendationStatusAsync(
                systemUnderTest,
                CategorySlug,
                SectionSlug,
                ChunkSlug,
                inputModel
            )
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.UpdateRecommendationStatus(
            CategorySlug,
            SectionSlug,
            ChunkSlug,
            inputModel
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task UpdateRecommendationStatus_WhenModelStateIsInvalid_ReturnsSingleRecommendationWithoutUpdating()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.ModelState.AddModelError("SelectedStatus", "Invalid status");
        var inputModel = new SingleRecommendationInputViewModel { SelectedStatus = "invalid" };
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
        var result = await systemUnderTest.UpdateRecommendationStatus(
            CategorySlug,
            SectionSlug,
            ChunkSlug,
            inputModel
        );

        // Assert
        Assert.Same(expectedResult, result);
        await _recommendationsViewBuilder
            .DidNotReceiveWithAnyArgs()
            .UpdateRecommendationStatusAsync(default!, default!, default!, default!, default!);
    }

    [Theory]
    [MemberData(nameof(InvalidSlugs))]
    public async Task UpdateRecommendationStatus_WhenAnySlugIsNullOrWhiteSpace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? chunkSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var inputModel = new SingleRecommendationInputViewModel { SelectedStatus = "Complete" };

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.UpdateRecommendationStatus(
                categorySlug!,
                sectionSlug!,
                chunkSlug!,
                inputModel
            )
        );
    }
}
