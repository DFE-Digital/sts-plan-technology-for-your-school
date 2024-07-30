using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers
{
    public class HeaderExtensionsTests
    {
        [Fact]
        public void Should_ReturnSmall_When_SmallTag()
        {
            var header = new Header()
            {
                Text = "Test",
                Size = HeaderSize.Small,
                Tag = HeaderTag.H1
            };

            var classForHeader = header.GetClassForSize();

            Assert.Equal(HeaderExtensions.SMALL, classForHeader);
        }

        [Fact]
        public void Should_ReturnMedium_When_MediumTag()
        {
            var header = new Header()
            {
                Text = "Test",
                Size = HeaderSize.Medium,
                Tag = HeaderTag.H1
            };

            var classForHeader = header.GetClassForSize();

            Assert.Equal(HeaderExtensions.MEDIUM, classForHeader);
        }

        [Fact]
        public void Should_ReturnLarge_When_LargeTag()
        {
            var header = new Header()
            {
                Text = "Test",
                Size = HeaderSize.Large,
                Tag = HeaderTag.H1
            };

            var classForHeader = header.GetClassForSize();

            Assert.Equal(HeaderExtensions.LARGE, classForHeader);
        }

        [Fact]
        public void Should_ReturnExtraLarge_When_ExtraLargeTag()
        {
            var header = new Header()
            {
                Text = "Test",
                Size = HeaderSize.ExtraLarge,
                Tag = HeaderTag.H1
            };

            var classForHeader = header.GetClassForSize();

            Assert.Equal(HeaderExtensions.EXTRALARGE, classForHeader);
        }
    }
}
