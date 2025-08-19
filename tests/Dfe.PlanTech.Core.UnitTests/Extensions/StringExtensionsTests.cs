using Dfe.PlanTech.Core.Extensions;
using Xunit;

namespace Dfe.PlanTech.Core.UnitTests.Extensions;

public class StringExtensionsTests
{
    #region FirstCharToUpper

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("Hello", "Hello")]
    [InlineData("h", "H")]
    [InlineData("1abc", "1abc")] // non-letter remains unchanged
    public void FirstCharToUpper_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = input.FirstCharToUpper();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FirstCharToUpper_Empty_ReturnsEmpty()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = input.FirstCharToUpper();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region FirstCharToLower

    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("H", "h")]
    [InlineData("1Abc", "1Abc")] // non-letter remains unchanged
    public void FirstCharToLower_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = input.FirstCharToLower();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FirstCharToLower_Empty_Throws()
    {
        // Arrange
        var input = string.Empty;

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => input.FirstCharToLower());
    }

    #endregion

    #region UseNonBreakingHyphenAndHtmlDecode

    [Fact]
    public void UseNonBreakingHyphenAndHtmlDecode_Null_ReturnsNull()
    {
        // Arrange
        string? input = null;

        // Act
        var result = input.UseNonBreakingHyphenAndHtmlDecode();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UseNonBreakingHyphenAndHtmlDecode_Empty_ReturnsEmpty()
    {
        // Arrange
        string? input = string.Empty;

        // Act
        var result = input.UseNonBreakingHyphenAndHtmlDecode();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void UseNonBreakingHyphenAndHtmlDecode_ReplacesHyphenWithNonBreakingHyphen()
    {
        // Arrange
        var input = "a-b";

        // Act
        var result = input.UseNonBreakingHyphenAndHtmlDecode();

        // Assert
        Assert.Equal("a\u2011b", result); // \u2011 is the non-breaking hyphen
    }

    [Fact]
    public void UseNonBreakingHyphenAndHtmlDecode_DecodesHtmlEntities_And_ReplacesHyphens()
    {
        // Arrange
        var input = "one-two &amp; three-four";
        var expected = "one\u2011two & three\u2011four";

        // Act
        var result = input.UseNonBreakingHyphenAndHtmlDecode();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void UseNonBreakingHyphenAndHtmlDecode_NoHyphen_StillDecodesEntities()
    {
        // Arrange
        var input = "&amp;";

        // Act
        var result = input.UseNonBreakingHyphenAndHtmlDecode();

        // Assert
        Assert.Equal("&", result);
    }

    #endregion

    #region Slugify

    // TODO: Confirm if the preservation of spaces around hyphens is intended behavior (leading to triple-spaces etc.).
    [Theory]
    [InlineData(" Hello World ", "hello-world")]
    [InlineData("Hello, World!", "hello-world")]
    [InlineData("C# & .NET 8", "c--net-8")] // multiple non-alphanumeric chars remain as adjacent hyphens
    [InlineData("multi   space\tand\nnewline", "multi-space-and-newline")] // whitespace condensed to single hyphen
    [InlineData("Already-Slugified", "already-slugified")] // hyphens preserved
    [InlineData("under_score", "underscore")] // underscores removed
    [InlineData("a\tb\nc", "a-b-c")] // mixed whitespace
    [InlineData("a\t \n  \t\nb", "a-b")] // sequential mixed whitespace collapsed to single hyphen
    [InlineData("x \n\t \t\n y", "x-y")] // sequential mixed whitespace collapsed to single hyphen
    [InlineData("___a___b___", "ab")] // only underscores removed
    [InlineData("a - b", "a---b")] // whitespace replaced around literal hyphen leads to triple hyphen
    [InlineData("  -leading", "-leading")] // trim then hyphen preserved
    [InlineData("trailing-  ", "trailing-")] // trim then hyphen preserved
    [InlineData("!!!", "")] // only punctuation removed
    [InlineData("", "")] // empty stays empty
    [InlineData("C#--.NET!! 8", "c--net-8")] // multiple punctuation removed, adjacent hyphens remain
    [InlineData("Foo@#$Bar", "foobar")] // non-alnum removed, no hyphens introduced
    [InlineData("foo   ---   bar", "foo-----bar")] // spaces around literal hyphens condense, hyphens preserved
    [InlineData("-a-b-", "-a-b-")] // leading/trailing hyphens preserved
    public void Slugify_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = input.Slugify();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
