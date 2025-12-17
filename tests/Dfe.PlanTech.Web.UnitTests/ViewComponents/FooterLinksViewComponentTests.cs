using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using NSubstitute;

namespace Dfe.PlanTech.Web.Tests.ViewComponents
{
    public class FooterLinksViewComponentTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldReturnViewWithNavigationLinks()
        {
            var mockViewBuilder = Substitute.For<IFooterLinksViewComponentViewBuilder>();
            var expectedLinks = new List<NavigationLinkEntry>
            {
                new NavigationLinkEntry { DisplayText = "Cookies", Href = "/cookies" },
                new NavigationLinkEntry { DisplayText = "Contact", Href = "/contact" }
            };

            mockViewBuilder.GetNavigationLinksAsync().Returns(Task.FromResult(expectedLinks));

            var viewComponent = new FooterLinksViewComponent(mockViewBuilder);

            var result = await viewComponent.InvokeAsync();

            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData);
            Assert.Equal(expectedLinks, viewResult.ViewData.Model);

            await mockViewBuilder.Received(1).GetNavigationLinksAsync();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenViewBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new FooterLinksViewComponent(null!));
            Assert.Equal("viewBuilder", exception.ParamName);
        }
    }
}
