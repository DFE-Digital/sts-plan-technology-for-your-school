using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Cookie.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CookieControllerTests
    {
        private readonly Page[] _pages = new Page[]
        {
            new Page()
            {
                Slug = "cookies",
                Title = new Title() { Text = "Cookies" },
                Content = new ContentComponent[] { new Header() { Tag = Domain.Content.Enums.HeaderTag.H1, Text = "Analytical Cookies" }}
            },
        };


        public static CookiesController CreateStrut()
        {
            Mock<ILogger<CookiesController>> loggerMock = new Mock<ILogger<CookiesController>>();
            Mock<ICookieService> cookiesMock = new Mock<ICookieService>();
            Mock<ITrackingConsentFeature> trackingConsentMock = new Mock<ITrackingConsentFeature>();

            return new CookiesController(loggerMock.Object, cookiesMock.Object);
        }

        [Theory]
        [InlineData("https://localhost:8080/self-assessment")]
        [InlineData("https://www.dfe.gov.uk/self-assessment")]
        public void HideBanner_Redirects_BackToPlaceOfOrigin(string url)
        {
            //Arrange
            var strut = CreateStrut();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = url;

            strut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            //Act
            var result = strut.HideBanner() as RedirectResult;

            //Assert
            Assert.IsType<RedirectResult>(result);
            Assert.Equal(url, result.Url);
        }

        [Theory]
        [InlineData("https://localhost:8080/self-assessment")]
        [InlineData("https://www.dfe.gov.uk/self-assessment")]
        public void Accept_Redirects_BackToPlaceOfOrigin(string url)
        {
            //Arrange
            var strut = CreateStrut();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = url;
            strut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            //Act
            var result = strut.Accept() as RedirectResult;

            //Assert
            Assert.IsType<RedirectResult>(result);
            Assert.Equal(url, result.Url);
        }

        private static IRequestCookieCollection MockRequestCookieCollection(string key, string value)
        {
            var requestFeature = new HttpRequestFeature();
            var featureCollection = new FeatureCollection();

            requestFeature.Headers = new HeaderDictionary();
            requestFeature.Headers.Add(HeaderNames.Cookie, new StringValues(key + "=" + value));

            featureCollection.Set<IHttpRequestFeature>(requestFeature);

            var cookiesFeature = new RequestCookiesFeature(featureCollection);

            return cookiesFeature.Cookies;
        }
        
        
         [Fact]
         public async Task CookiesPageDisplays()
         {
            Mock<IQuestionnaireCacher> questionnaireCacherMock = new Mock<IQuestionnaireCacher>();
            Mock<IContentRepository> contentRepositoryMock = SetupRepositoryMock();
            Mock<GetPageQuery> _getPageQueryMock = new Mock<GetPageQuery>(questionnaireCacherMock.Object, contentRepositoryMock.Object);

            CookiesController cookiesController = CreateStrut();
            var result = cookiesController.GetCookiesPage(_getPageQueryMock.Object);
            Assert.IsType<ViewResult>(result);
             
            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("Cookies", viewResult.ViewName);
         }
         
         [Theory]
         [InlineData("yes")]
         [InlineData("no")]
         public void settingCookiePreferenceBasedOnInputRedirectsToCookiePage(string userPreference)
         {
             CookiesController cookiesController = CreateStrut();
             
             var tempDataMock = new Mock<ITempDataDictionary>();
             var httpContextMock = new Mock<HttpContext>();
             var responseMock = new Mock<HttpResponse>();
             var cookiesMock = new Mock<IResponseCookies>();


             responseMock.SetupGet(r => r.Cookies).Returns(cookiesMock.Object);
             httpContextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

             cookiesController.TempData = tempDataMock.Object;
             cookiesController.ControllerContext = new ControllerContext()
             {
                 HttpContext = httpContextMock.Object
             };


             var result = cookiesController.CookiePreference(userPreference);

             if (userPreference == "yes")
             {
                 cookiesMock.Verify(c => c.Append("cookies_preferences_set", "true", It.IsAny<CookieOptions>()),
                     Times.Once);
             }
             else
             {
                 cookiesMock.Verify(c => c.Append("cookies_preferences_set", "false", It.IsAny<CookieOptions>()),
                     Times.Once);
             }
             
             tempDataMock.VerifySet(td => td["UserPreferenceRecorded"] = true, Times.Once);
             
             Assert.IsType<RedirectToActionResult>(result);

             var res = result as RedirectToActionResult;

             if (res != null)
             {
                 Assert.True(res.ActionName == "GetByRoute");
                 Assert.True(res.ControllerName == "Pages");
             }
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
    }
}
