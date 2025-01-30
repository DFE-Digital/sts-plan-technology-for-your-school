using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("Single test-", "Single test‑")]
        [InlineData("Single-test", "Single‑test")]
        [InlineData("This is -a-test-", "This is ‑a‑test‑")]
        [InlineData("This is-a-test", "This is‑a‑test")]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void CheckHyphensConvertedCorrectly(string inputText, string expectedText)
        {
            var result = StringExtensions.UseNonBreakingHyphenAndHtmlDecode(inputText);

            Assert.Equal(expectedText, result);
        }
    }
}
