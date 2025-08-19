using System.Text;
using Dfe.PlanTech.Core.Extensions;
using Xunit;

namespace Dfe.PlanTech.Core.UnitTests.Extensions;

public class StringBuilderExtensionsTests
{
    #region EndsWith

    [Fact]
    public void EndsWith_ReturnsTrue_WhenSuffixMatches()
    {
        // Arrange
        var sb = new StringBuilder("hello world");
        var test = "world";

        // Act
        var result = sb.EndsWith(test);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EndsWith_ReturnsFalse_WhenSuffixDoesNotMatch()
    {
        // Arrange
        var sb = new StringBuilder("hello world");
        var test = "planet";

        // Act
        var result = sb.EndsWith(test);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EndsWith_ReturnsFalse_WhenTestLongerThanBuilder()
    {
        // Arrange
        var sb = new StringBuilder("hi");
        var test = "hello";

        // Act
        var result = sb.EndsWith(test);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EndsWith_IsCaseSensitive()
    {
        // Arrange
        var sb = new StringBuilder("Hello");
        var test = "hello";

        // Act
        var result = sb.EndsWith(test);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EndsWith_EmptyTestString_ReturnsTrue()
    {
        // Arrange
        var sb = new StringBuilder("anything");
        var test = string.Empty;

        // Act
        var result = sb.EndsWith(test);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EndsWith_FullLengthMatch_ReturnsTrue()
    {
        // Arrange
        var sb = new StringBuilder("match");
        var test = "match";

        // Act
        var result = sb.EndsWith(test);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EndsWith_NullTest_Throws()
    {
        // Arrange
        var sb = new StringBuilder("text");
        string? test = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => sb.EndsWith(test!));
    }

    #endregion
}
