using System.Security.Claims;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class PagesControllerTests
    {
        private const string INDEX_SLUG = "/";
        private const string INDEX_TITLE = "Index";
        private const string SELF_ASSESSMENT_SLUG = "self-assessment";
        private const string INTERNAL_ERROR_ID = "InternalError";
        readonly ICookieService cookiesSubstitute = Substitute.For<ICookieService>();
        readonly IUser userSubstitute = Substitute.For<IUser>();
        private readonly IGetNavigationQuery _getNavigationQuery = Substitute.For<IGetNavigationQuery>();
        private readonly PagesController _controller;
        private readonly ControllerContext _controllerContext;
        private readonly IOptions<ContactOptions> _contactOptions;
        private readonly IOptions<ErrorPages> _errorPages;

        public PagesControllerTests()
        {
            var Logger = Substitute.For<ILogger<PagesController>>();

            _controllerContext = ControllerHelpers.SubstituteControllerContext();
            _getNavigationQuery.GetLinkById(Arg.Any<string>()).Returns(new NavigationLink { DisplayText = "contact us", Href = "/contact-us", OpenInNewTab = true });

            var contactUs = new ContactOptions
            {
                LinkId = "LinkId"
            };
            _contactOptions = Options.Create(contactUs);
            _errorPages = Options.Create(new ErrorPages { InternalErrorPageId = INTERNAL_ERROR_ID });

            _controller = new PagesController(Logger, _getNavigationQuery, _contactOptions, _errorPages)
            {
                ControllerContext = _controllerContext,
                TempData = Substitute.For<ITempDataDictionary>()
            };

            var claimIdentity = new ClaimsIdentity(new[] { new Claim("Type", "Value") }, CookieAuthenticationDefaults.AuthenticationScheme);
            _controllerContext.HttpContext.User.Identity.Returns(claimIdentity);
        }

        [Fact]
        public async Task Should_ReturnLandingPage_When_IndexRouteLoaded()
        {
            var cookie = new DfeCookie { UserAcceptsCookies = true };
            cookiesSubstitute.Cookie.Returns(cookie);

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

            var page = new Page()
            {
                Sys = new SystemDetails { Id = "index-id" },
                Slug = INDEX_SLUG,
                Title = new Title()
                {
                    Text = INDEX_TITLE
                }
            };

            var result = await _controller.GetByRoute(page, userSubstitute);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.Equal(INDEX_SLUG, asPage!.Page.Slug);
            Assert.Contains(INDEX_TITLE, asPage!.Page.Title!.Text);
        }

        [Fact]
        public async Task Should_SetOrganisationName_When_DisplayOrganisationNameIsTrue()
        {
            var cookie = new DfeCookie { UserAcceptsCookies = true };
            cookiesSubstitute.Cookie.Returns(cookie);

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

            var page = new Page()
            {
                Sys = new SystemDetails { Id = "self-assessment-id" },
                Slug = SELF_ASSESSMENT_SLUG,
                Title = new Title()
                {
                    Text = "Self assessment"
                },
                DisplayOrganisationName = true
            };

            var result = await _controller.GetByRoute(page, userSubstitute);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.Equal(establishment.OrgName, asPage!.Page.OrganisationName);
            Assert.Equal(SELF_ASSESSMENT_SLUG, asPage!.Page.Slug);
        }

        [Fact]
        public async Task Should_Not_OrganisationName_When_DisplayOrganisationName_Is_False()
        {
            var cookie = new DfeCookie { UserAcceptsCookies = true };
            cookiesSubstitute.Cookie.Returns(cookie);

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

            var page = new Page()
            {
                Sys = new SystemDetails { Id = "self-assessment-id" },
                Slug = SELF_ASSESSMENT_SLUG,
                Title = new Title()
                {
                    Text = "Self assessment"
                },
                DisplayOrganisationName = false
            };

            var result = await _controller.GetByRoute(page, userSubstitute);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.Null(asPage!.Page.OrganisationName);
            Assert.Equal(SELF_ASSESSMENT_SLUG, asPage!.Page.Slug);
        }

        [Fact]
        public async Task Should_Disable_Blue_Banner_For_ServerError_Page()
        {
            var page = new Page()
            {
                Sys = new SystemDetails { Id = INTERNAL_ERROR_ID },
            };

            var result = await _controller.GetByRoute(page, userSubstitute);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.NotNull(asPage);
            Assert.False(asPage.DisplayBlueBanner);
        }

        [Fact]
        public async Task Should_Not_Disable_Blue_Banner_For_Non_Error_Pages()
        {
            var page = new Page()
            {
                Sys = new SystemDetails { Id = "normal-page" },
            };

            var result = await _controller.GetByRoute(page, userSubstitute);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<PageViewModel>(model);

            var asPage = model as PageViewModel;
            Assert.NotNull(asPage);
            Assert.True(asPage.DisplayBlueBanner);
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
        public async Task Should_ReturnNotFoundError_Page_When_Page_Is_Null()
        {
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

            var result = await _controller.GetByRoute(null, userSubstitute) as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_Render_NotFound_Page()
        {
            var httpContextSubstitute = Substitute.For<HttpContext>();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextSubstitute
            };
            _controller.ControllerContext = controllerContext;
            var result = _controller.NotFoundError();
            var viewResult = await result as ViewResult;
            Assert.NotNull(viewResult);
        }
    }
}
