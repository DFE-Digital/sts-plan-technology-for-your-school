using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Cookie.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class PagesControllerTests
    {
        private const string INDEX_SLUG = "/";
        private const string INDEX_TITLE = "Index";
        ICookieService cookiesSubstitute = Substitute.For<ICookieService>();
        IUser userSubstitute = Substitute.For<IUser>();

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
            },
            new Page()
            {
                Slug = "accessibility",
                Title = new Title()
                {
                    Text = "Accessibility Page"
                },
                Content = Array.Empty<IContentComponent>()
            },
            new Page()
            {
                Slug = "privacy-policy",
                Title = new Title()
                {
                    Text = "Privacy Policy Page"
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

            var controllerContext = ControllerHelpers.SubstituteControllerContext();

            _controller = new PagesController(Logger)
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
            
            var establishment = new EstablishmentDto()
            {
                OrgName = "Test Org",
                Ukprn = "12345678",
                Urn = "123456",
                Type = new EstablishmentTypeDto()
                {
                    Name = "Test Name"
                }
            };
            
            userSubstitute.GetOrganisationData().Returns(establishment);

            var result = await _controller.GetByRoute(INDEX_SLUG, _query, userSubstitute, CancellationToken.None);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.Equal(establishment.OrgName, asPage!.OrganisationName);
            Assert.Equal(INDEX_SLUG, asPage!.Page.Slug);
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
                _controller.GetByRoute("NOT A VALID ROUTE", _query, userSubstitute, CancellationToken.None));
        }

        [Fact]
        public async Task Should_Retrieve_AccessibilityPage()
        {
            var httpContextSubstitute = Substitute.For<HttpContext>();

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextSubstitute
            };

            _controller.ControllerContext = controllerContext;

            var result = _controller.GetAccessibilityPage(_query);

            var viewResult = await result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);
        }

        [Fact]
        public async Task Should_Retrieve_PrivacyPolicyPage()
        {
            var httpContextSubstitute = Substitute.For<HttpContext>();

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextSubstitute
            };

            _controller.ControllerContext = controllerContext;

            var result = _controller.GetPrivacyPolicyPage(_query);

            var viewResult = await result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);
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

        [Fact]
        public void Should_Render_Service_Unavailable_Page()
        {
            var httpContextSubstitute = Substitute.For<HttpContext>();

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextSubstitute
            };

            _controller.ControllerContext = controllerContext;

            var result = _controller.ServiceUnavailable();

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<ServiceUnavailableViewModel>(model);
        }
    }
}