using Dfe.PlanTech.Web.Helpers;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class ViewHelpersTests
{
    [Theory]
    [InlineData("Hello, World!", "hello-world")] // punctuation stripped, space -> hyphen
    [InlineData("C# & .NET 9", "c-net-9")] // symbols stripped, digits kept
    [InlineData("Room 101", "room-101")] // digits preserved
    [InlineData("Under_score and-dash", "underscore-anddash")] // '_' and '-' stripped, spaces -> hyphens
    public void Slugify_CommonCases(string input, string expected)
    {
        var slug = input.Slugify();
        Assert.Equal(expected, slug);
    }

    [Fact]
    public void Slugify_MultipleSpaces_ProduceMultipleHyphens()
    {
        var input = "  Multiple   spaces  ";
        var slug = input.Slugify();
        // Leading and trailing spaces removed, multiple spaces become a single hyphen
        Assert.Equal("multiple-spaces", slug);
    }

    [Fact]
    public void Slugify_AccentedLetters_AreRemoved_NotNormalized()
    {
        var input = "Málaga mañana";
        var slug = input.Slugify();
        // á -> removed, ñ -> removed
        Assert.Equal("mlaga-maana", slug);
    }

    [Fact]
    public void Slugify_Tabs_And_Newlines_AreNotConverted_ToHyphens()
    {
        var input = "A B\tC\nD"; // space, tab, newline
        var slug = input.Slugify();
        // Only spaces become '-'; tabs/newlines remain
        Assert.Equal("a-b\tc\nd", slug);
        Assert.Contains('\t', slug);
        Assert.Contains('\n', slug);
    }

    [Fact]
    public void Slugify_Null_Throws_ArgumentNullException()
    {
        string? input = null;
        Assert.Throws<ArgumentNullException>(() => ViewHelpers.Slugify(input!));
        // Explanation: Regex.Replace(null, ...) throws ArgumentNullException before any string.Replace runs.
    }
}
