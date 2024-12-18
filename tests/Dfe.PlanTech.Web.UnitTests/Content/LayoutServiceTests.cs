using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Web.Content;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Content
{
    public class LayoutServiceTests
    {
        private readonly LayoutService _layoutService = new();

        private readonly string Home = "Home";
        private readonly string About = "About";
        private readonly string Contact = "Contact";
        private readonly string HomeSlug = "Home Slug";
        private readonly string AboutSlug = "About Slug";
        private readonly string HomeTitle = "Home Title";
        private readonly string AboutTitle = "About Title";
        private readonly string HomeSubtitle = "Home Subtitle";
        private readonly string AboutSubtitle = "About Subtitle";

        private readonly string HeadingTitle = "Heading Title";
        private readonly string HeadingSubtitle = "Heading Subtitle";

        private CsPage GetPage()
        {
            return new CsPage
            {
                Heading = new CSHeading
                {
                    Title = HeadingTitle,
                    Subtitle = HeadingSubtitle
                },
                Content = new()
                {
                    new()
                    {
                        InternalName = Home, Slug = HomeSlug, Title = HomeTitle,
                        Subtitle = HomeSubtitle,
                        UseParentHero = true
                    },
                    new()
                    {
                        InternalName = About, Slug = AboutSlug, Title = AboutTitle,
                        Subtitle = AboutSubtitle,
                        UseParentHero = false
                    }
                }
            };
        }


        private static string GetSegmentLength(int length)
        {
            var segment = "";
            for (var i = 1; i <= length; i++)
            {
                segment += $"/segment{i}";
            }

            return segment;
        }


        [Fact]
        public void GetHeading_PageExists_ReturnsCorrectHeading()
        {
            // Arrange
            var page = GetPage();

            // Act
            var result = _layoutService.GetHeading(page, AboutSlug);

            // Assert
            Assert.Equal(AboutTitle, result.Title);
            Assert.Equal(AboutSubtitle, result.Subtitle);
        }


        [Fact]
        public void GetHeading_PageDoesNotExist_ReturnsParentHero()
        {
            // Arrange
            var page = GetPage();

            // Act
            var result = _layoutService.GetHeading(page, Contact);

            // Assert
            result.Title.Should().Be(HeadingTitle);
            result.Subtitle.Should().Be(HeadingSubtitle);
        }


        [Fact]
        public void GenerateVerticalNavigation_PageNameMatches_ReturnsCorrectMenuItems()
        {
            // Arrange
            var page = GetPage();

            var request = new DefaultHttpContext().Request;

            // Act
            var result = _layoutService.GenerateVerticalNavigation(page, request, AboutSlug);

            // Assert
            Assert.Equal(page.Content.Count, result.Count);
            Assert.Equal(AboutTitle, result[1].Title);
            Assert.True(result[1].IsActive);
        }


        [Fact]
        public void GenerateVerticalNavigation_PageNameDoesNotMatch_ReturnsMenuItemsWithFirstActive()
        {
            // Arrange
            var page = GetPage();

            var request = new DefaultHttpContext().Request;

            // Act
            var result = _layoutService.GenerateVerticalNavigation(page, request, Contact);

            // Assert
            Assert.Equal(page.Content.Count, result.Count);
            Assert.Equal(HomeTitle, result[0].Title);
            Assert.Equal(0, result.Count(o => o.IsActive));
        }


        [Fact]
        public void GetVisiblePageList_PageNameProvidedAndMatches_ReturnsMatchingItems()
        {
            // Arrange
            var page = GetPage();

            // Act
            var result = LayoutService.GetVisiblePageList(page, AboutSlug);

            // Assert
            Assert.Single(result);
            Assert.Equal(About, result[0].InternalName);
        }


        [Fact]
        public void GetVisiblePageList_PageNameProvidedAndDoesNotMatch_ReturnsEmptyList()
        {
            // Arrange
            var page = GetPage();

            // Act
            var result = LayoutService.GetVisiblePageList(page, Contact);

            // Assert
            Assert.Empty(result);
        }


        [Fact]
        public void GetVisiblePageList_PageNameIsNullOrEmpty_ReturnsFirstItem()
        {
            // Arrange
            var page = GetPage();

            // Act
            var result = LayoutService.GetVisiblePageList(page, string.Empty);

            // Assert
            Assert.Single(result);
            Assert.Equal(Home, result[0].InternalName);
        }


        [Fact]
        public void GetVisiblePageList_ContentListIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var page = GetPage();
            page.Content = new();

            // Act
            var result = LayoutService.GetVisiblePageList(page, Home);

            // Assert
            Assert.Empty(result);
        }


        [Fact]
        public void GetNavigationUrl_MoreThanTwoSegments_ReturnsFirstTwoSegments()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = GetSegmentLength(4);

            // Act
            var result = LayoutService.GetNavigationUrl(context.Request);

            // Assert
            Assert.Equal(GetSegmentLength(2), result);
        }


        [Fact]
        public void GetNavigationUrl_ExactlyTwoSegments_ReturnsAllSegments()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = GetSegmentLength(2);

            // Act
            var result = LayoutService.GetNavigationUrl(context.Request);

            // Assert
            Assert.Equal(GetSegmentLength(2), result);
        }


        [Fact]
        public void GetNavigationUrl_FewerThanTwoSegments_ReturnsAllSegments()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = GetSegmentLength(1);

            // Act
            var result = LayoutService.GetNavigationUrl(context.Request);

            // Assert
            Assert.Equal(GetSegmentLength(1), result);
        }


        [Fact]
        public void GetNavigationUrl_EmptyUrl_ReturnsEmptyString()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var emptyRequestPath = string.Empty;
            context.Request.Path = emptyRequestPath;

            // Act
            var result = LayoutService.GetNavigationUrl(context.Request);

            // Assert
            Assert.Equal(emptyRequestPath, result);
        }


        [Fact]
        public void GetHeading_NoPage_ReturnsParentHero()
        {
            var page = GetPage();

            var result = _layoutService.GetHeading(page, string.Empty);

            result.Title.Should().Be(HeadingTitle);
            result.Subtitle.Should().Be(HeadingSubtitle);
        }

        [Fact]
        public void GetHeading_Page_UseParent_ReturnsParentHero()
        {
            var page = GetPage();

            var result = _layoutService.GetHeading(page, HomeSlug);

            result.Title.Should().Be(HeadingTitle);
            result.Subtitle.Should().Be(HeadingSubtitle);
        }

        [Fact]
        public void GetHeading_Page_DontUseParent_ReturnsParentHero()
        {
            var page = GetPage();

            var result = _layoutService.GetHeading(page, AboutSlug);

            result.Title.Should().Be(AboutTitle);
            result.Subtitle.Should().Be(AboutSubtitle);
        }
    }
}
