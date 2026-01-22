using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
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
    public class CookieControllerTests
    {
        private readonly Page[] _pages =
        [
            new Page()
            {
                Slug = "cookies",
                Title = new Title() { Text = "Cookies" },
                Content = [new Header() { Tag = HeaderTag.H1, Text = "Analytical Cookies" }],
            },
        ];

        public static CookiesController CreateStrut()
        {
            ILogger<CookiesController> loggerSubstitute = Substitute.For<
                ILogger<CookiesController>
            >();
            ICookieService cookiesSubstitute = Substitute.For<ICookieService>();

            return new CookiesController(loggerSubstitute, cookiesSubstitute)
            {
                ControllerContext = ControllerHelpers.SubstituteControllerContext(),
            };
        }

        [Theory]
        [InlineData("https://localhost:8080/homet")]
        [InlineData("https://www.dfe.gov.uk/home")]
        public void HideBanner_Redirects_BackToPlaceOfOrigin(string url)
        {
            //Arrange
            var strut = CreateStrut();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Referer = url;

            strut.ControllerContext = new ControllerContext { HttpContext = httpContext };

            //Act
            var result = strut.HideBanner() as RedirectResult;

            //Assert
            Assert.IsType<RedirectResult>(result);
            Assert.Equal(url, result.Url);
        }

        [Theory]
        [InlineData("https://localhost:8080/home", "true")]
        [InlineData("https://www.dfe.gov.uk/home", "false")]
        public void SetCookiePreference_Redirects_BackToPlaceOfOrigin(
            string url,
            string userAcceptsCookie
        )
        {
            //Arrange
            var strut = CreateStrut();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Referer = url;
            strut.ControllerContext = new ControllerContext { HttpContext = httpContext };

            //Act
            var result = strut.SetCookiePreference(userAcceptsCookie) as RedirectResult;

            //Assert
            Assert.IsType<RedirectResult>(result);
            Assert.Equal(url, result.Url);
        }

        [Theory]
        [InlineData("https://www.google.com")]
        [InlineData("https://www.plantech.education.gov.uk/accessibility")]
        public async Task ReferrerUrl_Should_Be_Retrieved_From_Header(string referrerUrl)
        {
            IGetPageQuery getPageQuery = SetupPageQueryMock();

            CookiesController cookiesController = CreateStrut();
            cookiesController.HttpContext.Request.Headers.Referer = referrerUrl;

            var result = await cookiesController.GetCookiesPage(
                getPageQuery,
                CancellationToken.None
            );
            Assert.IsType<ViewResult>(result);

            var model = (result as ViewResult)!.Model as CookiesViewModel;

            Assert.NotNull(model);

            Assert.Equal(referrerUrl, model.ReferrerUrl);
        }

        [Fact]
        public async Task CookiesPageDisplays()
        {
            IGetPageQuery getPageQuery = SetupPageQueryMock();

            CookiesController cookiesController = CreateStrut();
            var result = await cookiesController.GetCookiesPage(
                getPageQuery,
                CancellationToken.None
            );
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("Cookies", viewResult.ViewName);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public void CookiePreferences_Redirects_ToCookiePage(string userPreference)
        {
            CookiesController cookiesController = CreateStrut();

            var tempDataSubstitute = Substitute.For<ITempDataDictionary>();

            cookiesController.TempData = tempDataSubstitute;

            var result = cookiesController.SetCookiePreference(userPreference, true);

            tempDataSubstitute.Received(1)[CookieConstants.UserPreferenceRecordedKey] = true;

            Assert.IsType<RedirectToActionResult>(result);

            if (result is RedirectToActionResult res)
            {
                Assert.Equal("GetByRoute", res.ActionName);
                Assert.Equal("Pages", res.ControllerName);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("1234")]
        [InlineData("bad-thing")]
        public void Invalid_Preference_Throws_Exception(string? userPreference)
        {
            CookiesController cookiesController = CreateStrut();

            var tempDataSubstitute = Substitute.For<ITempDataDictionary>();
            var cookiesSubstitute = Substitute.For<ICookieService>();

            cookiesSubstitute.SetCookieAcceptance(true);

            cookiesController.TempData = tempDataSubstitute;

            var result = Assert.Throws<ArgumentException>(() =>
                cookiesController.SetCookiePreference(userPreference!)
            );
            Assert.Contains("Can't convert preference", result.Message);
        }

        private IGetPageQuery SetupPageQueryMock()
        {
            var getPageQuery = Substitute.For<IGetPageQuery>();
            getPageQuery
                .GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(callinfo =>
                {
                    var slug = callinfo.ArgAt<string>(0);
                    return _pages.FirstOrDefault(page => page.Slug == slug);
                });
            return getPageQuery;
        }
    }
}
