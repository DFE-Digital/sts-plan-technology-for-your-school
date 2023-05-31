using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class PagesControllerTests
    {
        private readonly List<Page> _pages = new() {
            new Page()
            {
                Slug = "Landing",
                Title = new Title()
                {
                    Text = "Landing Page Title"
                },
                Content = Array.Empty<IContentComponent>()
            },
            new Page()
            {
                Slug = "Other Page",
                Title = new Title()
                {
                    Text = "Other Page Title"
                },
                Content = Array.Empty<IContentComponent>()
            }
        };
        /*
        [Fact]
        public async Task Should_ReturnLandingPage_When_IndexRouteLoaded()
        {
            var contentRepositoryMock = new Mock<IContentRepository>();
            contentRepositoryMock.Setup(repo => repo.GetEntities<Page>(It.IsAny<IEnumerable<IContentQuery>>(), It.IsAny<CancellationToken>())).ReturnsAsync((IEnumerable<IContentQuery> queries, CancellationToken cancellationToken) =>
            {
                foreach (var query in queries)
                {
                    if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                    {
                        return _pages.Where(page => page.Slug == equalsQuery.Value);
                    }
                }

                return Array.Empty<Page>();
            });

            var mockLogger = new Mock<ILogger<PagesController>>();
            var controller = new PagesController(mockLogger.Object);

            var result = await controller.Index(new GetPageQuery(contentRepositoryMock.Object));

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<Page>(model);

            var asPage = model as Page;
            Assert.Equal("Landing", asPage!.Slug);
            Assert.Contains("Landing Page", asPage!.Title!.Text);
        }
        */
    }
}