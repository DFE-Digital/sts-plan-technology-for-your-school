using Dfe.PlanTech.Core.Constants;

namespace Dfe.PlanTech.Core.UnitTests.Constants;

public class RecommendationConstantsTests
{
    [Fact]
    public void StatusDisplayNames_KeysAreExactlyTheValidStatusKeys()
    {
        // Arrange
        var statusDisplayNamesKeys = RecommendationConstants.StatusDisplayNames.Keys.OrderBy(k => k);
        var validStatusKeys = RecommendationConstants.ValidStatusKeys.OrderBy(k => k);

        // Act & Assert
        Assert.Equal(validStatusKeys, statusDisplayNamesKeys);
    }

    [Fact]
    public void StatusDisplayNamesNonBreakingSpaces_KeysAreExactlyTheValidStatusKeys()
    {
        // Arrange
        var nonBreakingSpacesKeys = RecommendationConstants.StatusDisplayNamesNonBreakingSpaces.Keys.OrderBy(k => k);
        var validStatusKeys = RecommendationConstants.ValidStatusKeys.OrderBy(k => k);

        // Act & Assert
        Assert.Equal(validStatusKeys, nonBreakingSpacesKeys);
    }

    [Fact]
    public void StatusTagClasses_KeysAreExactlyTheValidStatusKeys()
    {
        // Arrange
        var statusTagClassesKeys = RecommendationConstants.StatusTagClasses.Keys.OrderBy(k => k);
        var validStatusKeys = RecommendationConstants.ValidStatusKeys.OrderBy(k => k);

        // Act & Assert
        Assert.Equal(validStatusKeys, statusTagClassesKeys);
    }

    [Fact]
    public void StatusDisplayNamesNonBreakingSpaces_ValuesMustNotHavePlainSpaces()
    {
        // Arrange
        var values = RecommendationConstants.StatusDisplayNamesNonBreakingSpaces.Values;

        // Act & Assert
        Assert.All(values, v => Assert.DoesNotContain(" ", v));
    }
}

