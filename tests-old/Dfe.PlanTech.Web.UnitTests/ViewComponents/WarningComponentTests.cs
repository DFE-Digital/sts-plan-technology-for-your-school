using Dfe.PlanTech.Web.UnitTests.Models;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents
{
    public class WarningComponentTests
    {
        public readonly string testText = "test text";
        public readonly string? nullText = null;
        public readonly string emptyText = "";

        [Theory]
        [InlineData("text")]
        [InlineData("longer text with spaces")]
        [InlineData("")]
        [InlineData(null)]
        public void WarningComponent_Sets_Text(string? text)
        {
            var testWarning = ComponentBuilder.BuildWarningComponent(text!);
            Assert.Equal(text, testWarning.Text.RichText.Value);
        }
    }
}
