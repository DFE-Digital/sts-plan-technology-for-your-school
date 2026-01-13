using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Web.Helpers;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class RichTextMarkExtensionsTests
{
    private class TestMark : IRichTextMark
    {
        public string Type { get; set; } = string.Empty;

        public MarkType MarkType => (MarkType)Enum.Parse(MarkType.GetType(), Type);
    }

    [Theory]
    [InlineData(RichTextMarkExtensions.UNDERLINE_MARK, RichTextMarkExtensions.UNDERLINE_CLASS)]
    [InlineData(RichTextMarkExtensions.BOLD_MARK, RichTextMarkExtensions.BOLD_CLASS)]
    public void GetClass_KnownMarks_ReturnsExpectedClass(string markType, string expectedClass)
    {
        // Arrange
        var mark = new TestMark { Type = markType };

        // Act
        var result = mark.GetClass();

        // Assert
        Assert.Equal(expectedClass, result);
    }

    [Fact]
    public void GetClass_UnknownMarks_ReturnsEmptyString()
    {
        // Arrange
        var mark = new TestMark { Type = "weird-one" };

        // Act
        var result = mark.GetClass();

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
