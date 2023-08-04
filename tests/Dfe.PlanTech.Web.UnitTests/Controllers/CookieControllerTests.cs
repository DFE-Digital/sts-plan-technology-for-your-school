using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CookieControllerTests
    {
        public static CookiesController CreateStrut()
        {
            return new CookiesController();
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
    }
}
