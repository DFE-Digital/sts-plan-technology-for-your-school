using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class PagesControllerTests
    {
        private const string INDEX_SLUG = "/";
        private const string INDEX_TITLE = "Index";

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
            },
            new Page(){
                Slug = INDEX_SLUG,
                Title = new Title(){
                    Text = INDEX_TITLE,
                },
                Content = Array.Empty<IContentComponent>()
                }
            };

        private readonly PagesController _controller;
        private readonly GetPageQuery _query;
        private readonly Mock<IQuestionnaireCacher> _questionnaireCacherMock = new Mock<IQuestionnaireCacher>();

        public PagesControllerTests()
        {
            Mock<IContentRepository> repositoryMock = SetupRepositoryMock();

            var mockLogger = new Mock<ILogger<PagesController>>();
            var historyMock = new Mock<IUrlHistory>();

            _controller = new PagesController(mockLogger.Object, historyMock.Object);

            _query = new GetPageQuery(_questionnaireCacherMock.Object, repositoryMock.Object);
        }

        private Mock<IContentRepository> SetupRepositoryMock()
        {
            var repositoryMock = new Mock<IContentRepository>();
            repositoryMock.Setup(repo => repo.GetEntities<Page>(It.IsAny<IGetEntitiesOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync((IGetEntitiesOptions options, CancellationToken _) =>
            {
                if (options?.Queries != null)
                {
                    foreach (var query in options.Queries)
                    {
                        if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                        {
                            return _pages.Where(page => page.Slug == equalsQuery.Value);
                        }
                    }
                }

                return Array.Empty<Page>();
            });
            return repositoryMock;
        }

        [Fact]
        public async Task Should_ReturnLandingPage_When_IndexRouteLoaded()
        {
            var result = await _controller.GetByRoute(INDEX_SLUG, _query, CancellationToken.None);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.Equal(INDEX_SLUG, asPage!.Page.Slug);
            Assert.Contains(INDEX_TITLE, asPage!.Page.Title!.Text);
        }

        [Fact]
        public async Task Should_Return_Page_On_Index_Route()
        {
            var result = await _controller.Index(_query, CancellationToken.None);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.Equal(INDEX_SLUG, asPage!.Page.Slug);
            Assert.Contains(INDEX_TITLE, asPage!.Page.Title!.Text);
        }

        [Fact]
        public async Task Should_ThrowError_When_NoRouteFound()
        {
            await Assert.ThrowsAnyAsync<Exception>(() => _controller.GetByRoute("NOT A VALID ROUTE", _query, CancellationToken.None));
        }
    }
}