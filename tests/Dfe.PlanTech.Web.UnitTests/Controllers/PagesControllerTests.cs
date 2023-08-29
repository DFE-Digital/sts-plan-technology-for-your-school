using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Cookie.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class PagesControllerTests
    {
        private const string INDEX_SLUG = "/";
        private const string INDEX_TITLE = "Index";
        ICookieService cookiesSubstitute = Substitute.For<ICookieService>();

        private readonly List<Page> _pages = new()
        {
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
            new Page()
            {
                Slug = INDEX_SLUG,
                Title = new Title()
                {
                    Text = INDEX_TITLE,
                },
                Content = Array.Empty<IContentComponent>()
            }
        };

        private readonly PagesController _controller;
        private readonly GetPageQuery _query;
        private IQuestionnaireCacher _questionnaireCacherSubstitute = Substitute.For<IQuestionnaireCacher>();

        public PagesControllerTests()
        {
            IContentRepository repositorySubstitute = SetupRepositorySubstitute();

            var Logger = Substitute.For<ILogger<PagesController>>();

            var config = new GtmConfiguration()
            {
                Head = "Test Head",
                Body = "Test Body"
            };

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "GTM:Head", config.Head },
                { "GTM:Body", config.Body },
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(initialData: inMemorySettings)
                .Build();

            var controllerContext = ControllerHelpers.SubstituteControllerContext();

            _controller = new PagesController(Logger, configuration, cookiesSubstitute)
            {
                ControllerContext = controllerContext,
                TempData = Substitute.For<ITempDataDictionary>()
            };

            _query = new GetPageQuery(_questionnaireCacherSubstitute, repositorySubstitute);
        }

        private IContentRepository SetupRepositorySubstitute()
        {
            var repositorySubstitute = Substitute.For<IContentRepository>();
            repositorySubstitute
                .GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>())
                .Returns((callInfo) =>
                {
                    IGetEntitiesOptions options = (IGetEntitiesOptions)callInfo[0]; 
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
            return repositorySubstitute;
        }

        [Fact]
        public async Task Should_ReturnLandingPage_When_IndexRouteLoaded()
        {
            var cookie = new DfeCookie { HasApproved = true };
            cookiesSubstitute.GetCookie().Returns(cookie);

            var result = await _controller.GetByRoute(INDEX_SLUG, _query, CancellationToken.None);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.Equal(INDEX_SLUG, asPage!.Page.Slug);
            Assert.Equal("Test Head", asPage!.GTMHead);
            Assert.Equal("Test Body", asPage!.GTMBody);
            Assert.Contains(INDEX_TITLE, asPage!.Page.Title!.Text);
        }

        [Fact]
        public async Task Should_Return_Page_On_Index_Route()
        {
            var cookie = new DfeCookie { HasApproved = true };
            cookiesSubstitute.GetCookie().Returns(cookie);
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
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _controller.GetByRoute("NOT A VALID ROUTE", _query, CancellationToken.None));
        }

        [Fact]
        public void Should_Retrieve_ErrorPage()
        {
            var httpContextSubstitute = Substitute.For<HttpContext>();

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextSubstitute
            };

            _controller.ControllerContext = controllerContext;

            var result = _controller.Error();

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<ErrorViewModel>(model);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("")]
        public async Task GoogleTrackingCodesAddedDependingOnWhatCookiePreferenceSetTo(string cookiePreference)
        {
            bool.TryParse(cookiePreference, out bool preference);
            var cookie = new DfeCookie { HasApproved = preference };
            cookiesSubstitute.GetCookie().Returns(cookie);

            var result = await _controller.GetByRoute(INDEX_SLUG, _query, CancellationToken.None);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            var asPage = model as PageViewModel;
            if (cookiePreference == "true")
            {
                Assert.Equal("Test Head", asPage!.GTMHead);
                Assert.Equal("Test Body", asPage!.GTMBody);
            }
            else
            {
                Assert.NotEqual("Test Head", asPage!.GTMHead);
                Assert.NotEqual("Test Body", asPage!.GTMBody);
            }
        }
    }
}