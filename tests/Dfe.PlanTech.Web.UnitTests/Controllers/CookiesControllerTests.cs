using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CookiesControllerTests
    {
        private readonly ILogger<CookiesController> _logger = Substitute.For<
            ILogger<CookiesController>
        >();
        private readonly IContentfulService _contentfulService =
            Substitute.For<IContentfulService>();
        private readonly ICookieService _cookieService = Substitute.For<ICookieService>();
        private readonly CookiesController _controller;

        public CookiesControllerTests()
        {
            _controller = new CookiesController(_logger, _contentfulService, _cookieService);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            };
        }

        [Fact]
        public async Task GetCookiesPage_ReturnsViewWithCorrectModel()
        {
            var pageContent = new PageEntry
            {
                Title = new ComponentTitleEntry("Test title"),
                Content = new List<ContentfulEntry>(),
            };

            var testCookie = new DfeCookieModel();

            _contentfulService.GetPageBySlugAsync("cookies").Returns(Task.FromResult(pageContent));
            _cookieService.Cookie.Returns(new DfeCookieModel());

            _controller.HttpContext.Request.Headers["Referer"] = "http://cookietests.com";

            var result = await _controller.GetCookiesPage();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CookiesViewModel>(viewResult.Model);
            Assert.Equal("Test title", model.Title.Text);
            Assert.Equal(testCookie, model.Cookie);
            Assert.Equal("http://cookietests.com", model.ReferrerUrl);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void SetCookiePreference_ValidPreference_SetsCookieAndRedirects(
            string input,
            bool expected
        )
        {
            _controller.HttpContext.Request.Headers["Referer"] = "http://cookietests.com";

            var result = _controller.SetCookiePreference(input);

            _cookieService.Received().SetCookieAcceptance(expected);
            var redirect = Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public void SetCookiePreference_InvalidPreference_ThrowsArgumentException()
        {
            var invalidParam = "not a bool";
            var ex = Assert.Throws<ArgumentException>(() =>
                _controller.SetCookiePreference(invalidParam)
            );
            Assert.Equal($"Can't convert preference (Parameter '{invalidParam}')", ex.Message);
        }

        [Fact]
        public void SetCookiePreference_FromCookiesPage_SetsTempDataAndRedirectsToCookies()
        {
            _controller.TempData = new TempDataDictionary(
                _controller.HttpContext,
                Substitute.For<ITempDataProvider>()
            );

            var result = _controller.SetCookiePreference("true", isCookiesPage: true);

            Assert.True(_controller.TempData.ContainsKey("UserPreferenceRecorded"));
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetByRoute", redirect.ActionName);
            Assert.Equal("Pages", redirect.ControllerName);
            Assert.NotNull(redirect.RouteValues);
            Assert.Equal("cookies", redirect.RouteValues["route"]);
        }

        [Fact]
        public void HideBanner_SetsVisibilityFalseAndRedirects()
        {
            _controller.HttpContext.Request.Headers["Referer"] = "http://cookietests.com";

            var result = _controller.HideBanner();

            _cookieService.Received().SetVisibility(false);
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("http://cookietests.com", redirect.Url);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContentfulServiceIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new CookiesController(_logger, null!, _cookieService)
            );
            Assert.Equal("contentfulService", ex.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenCookieServiceIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new CookiesController(_logger, _contentfulService, null!)
            );
            Assert.Equal("cookieService", ex.ParamName);
        }

        [Fact]
        public async Task GetCookiesPage_UsesFallbackTitle_WhenTitleIsNull()
        {
            var pageContent = new PageEntry { Title = null, Content = new List<ContentfulEntry>() };

            _contentfulService.GetPageBySlugAsync("cookies").Returns(pageContent);
            _cookieService.Cookie.Returns(new DfeCookieModel());
            _controller.HttpContext.Request.Headers["Referer"] = "http://cookietests.com";

            var result = await _controller.GetCookiesPage();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CookiesViewModel>(viewResult.Model);
            Assert.Equal("Cookies", model.Title.Text);
            Assert.Equal("http://cookietests.com", model.ReferrerUrl);
        }

        [Fact]
        public async Task GetCookiesPage_UsesFallbackContent_WhenContentIsNull()
        {
            var pageContent = new PageEntry
            {
                Title = new ComponentTitleEntry("Test title"),
                Content = null,
            };

            _contentfulService.GetPageBySlugAsync("cookies").Returns(pageContent);
            _cookieService.Cookie.Returns(new DfeCookieModel());
            _controller.HttpContext.Request.Headers["Referer"] = "http://cookietests.com";

            var result = await _controller.GetCookiesPage();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CookiesViewModel>(viewResult.Model);
            Assert.Equal("Test title", model.Title.Text);
            Assert.Empty(model.Content);
        }

        [Fact]
        public async Task GetCookiesPage_UsesEmptyReferrer_WhenHeaderIsMissing()
        {
            var pageContent = new PageEntry
            {
                Title = new ComponentTitleEntry("Test title"),
                Content = new List<ContentfulEntry>(),
            };

            _contentfulService.GetPageBySlugAsync("cookies").Returns(pageContent);
            _cookieService.Cookie.Returns(new DfeCookieModel());

            var result = await _controller.GetCookiesPage();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CookiesViewModel>(viewResult.Model);
            Assert.Equal("", model.ReferrerUrl);
        }

        [Fact]
        public async Task GetCookiesPage_HandlesNullCookie()
        {
            var pageContent = new PageEntry
            {
                Title = new ComponentTitleEntry("Test title"),
                Content = new List<ContentfulEntry>(),
            };

            _contentfulService.GetPageBySlugAsync("cookies").Returns(pageContent);
            _cookieService.Cookie.Returns(default(DfeCookieModel));

            var result = await _controller.GetCookiesPage();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CookiesViewModel>(viewResult.Model);
            Assert.Equal(default(DfeCookieModel), model.Cookie);
        }
    }
}
