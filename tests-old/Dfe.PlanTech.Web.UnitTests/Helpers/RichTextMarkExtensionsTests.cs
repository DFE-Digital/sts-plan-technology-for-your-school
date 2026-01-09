using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers
{
    public class RichTextMarkExtensionsTests
    {
        [Fact]
        public void Should_ReturnBold_When_Bold()
        {
            var mark = new RichTextMark() { Type = RichTextMarkExtensions.BOLD_MARK };

            var classForMark = mark.GetClass();

            Assert.Equal(RichTextMarkExtensions.BOLD_CLASS, classForMark);
        }

        [Fact]
        public void Should_ReturnUnderline_When_Underline()
        {
            var mark = new RichTextMark() { Type = RichTextMarkExtensions.UNDERLINE_MARK };

            var classForMark = mark.GetClass();

            Assert.Equal(RichTextMarkExtensions.UNDERLINE_CLASS, classForMark);
        }

        [Fact]
        public void Should_ReturnBlank_When_UnknownType()
        {
            var mark = new RichTextMark() { Type = "Not a real type!" };

            var classForMark = mark.GetClass();

            Assert.Equal("", classForMark);
        }
    }
}
