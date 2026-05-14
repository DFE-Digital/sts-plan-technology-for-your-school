using Dfe.PlanTech.Web.UnitTests.Models;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents
{
    public class NotificationBannerTests
    {
        [Theory]
        [InlineData("text")]
        [InlineData("longer text with spaces")]
        [InlineData("")]
        public void NotificationBanner_Text_Test(string text)
        {
            var banner = ComponentBuilder.BuildNotificationBanner(text);
            Assert.Equal(text, banner.Text.RichText.Value);
        }
    }
}
