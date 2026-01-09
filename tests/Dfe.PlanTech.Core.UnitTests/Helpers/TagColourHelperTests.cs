using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.UnitTests.Helpers;

public class TagColourHelperTests
{
    [Fact]
    public void GetMatchingColour_ReturnsDefault_WhenInputIsNull()
    {
        var result = TagColourHelper.GetMatchingColour(null);
        Assert.Equal(TagColourHelper.Default, result);
    }

    [Fact]
    public void GetMatchingColour_ReturnsDefault_WhenInputIsEmpty()
    {
        var result = TagColourHelper.GetMatchingColour(string.Empty);
        Assert.Equal(TagColourHelper.Default, result);
    }

    [Fact]
    public void GetMatchingColour_ReturnsDefault_WhenInputDoesNotMatchAnyColour()
    {
        var result = TagColourHelper.GetMatchingColour("arbitrary-colour");
        Assert.Equal(TagColourHelper.Default, result);
    }

    [Theory]
    [InlineData("blue", "blue")]
    [InlineData("grey", "grey")]
    [InlineData("light-blue", "light-blue")]
    [InlineData("red", "red")]
    [InlineData("green", "green")]
    [InlineData("yellow", "yellow")]
    public void GetMatchingColour_ReturnsCorrectColour_WhenInputMatchesExactly(
        string input,
        string expected
    )
    {
        var result = TagColourHelper.GetMatchingColour(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("BLUE", "blue")]
    [InlineData("Blue", "blue")]
    [InlineData("BlUe", "blue")]
    [InlineData("bLuE", "blue")]
    [InlineData("blue", "blue")]
    public void GetMatchingColour_ReturnsCorrectColour_WhenInputMatchesCaseInsensitively(
        string input,
        string expected
    )
    {
        var result = TagColourHelper.GetMatchingColour(input);
        Assert.Equal(expected, result);
    }
}
