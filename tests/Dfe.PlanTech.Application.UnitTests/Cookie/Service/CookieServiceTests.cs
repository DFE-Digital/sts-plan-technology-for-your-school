using Dfe.PlanTech.Application.Cookie.Service;
using Dfe.PlanTech.Domain.Cookie;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Text.Json;


namespace Dfe.PlanTech.Application.UnitTests.Cookie.Service
{
    public class CookieServiceTests
    {
        IHttpContextAccessor mockHttp = Substitute.For<IHttpContextAccessor>();

        private CookieService CreateStrut()
        {
            return new CookieService(mockHttp);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetVisibility_Sets_Cookie_Visbility(bool visibility)
        {
            var cookieSerialized = SerializeCookie(visibility, false, false);
            SetUpCookie(cookieSerialized);

            var service = CreateStrut();
            service.SetVisibility(visibility);

            var cookie = service.GetCookie();
            Assert.IsType<DfeCookie>(cookie);
            Assert.Equal(visibility, cookie.IsVisible);
            Assert.False(cookie.HasApproved);
            Assert.False(cookie.IsRejected);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RejectCookies_Sets_Cookie_To_Rejected(bool isRejected)
        {
            var cookieSerialized = SerializeCookie(true, isRejected, false);
            SetUpCookie(cookieSerialized);
            var service = CreateStrut();
            service.RejectCookies();

            var cookie = service.GetCookie();
            Assert.IsType<DfeCookie>(cookie);
            Assert.Equal(isRejected, cookie.IsRejected);
            Assert.False(cookie.HasApproved);
            Assert.True(cookie.IsVisible);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetPreference_Sets_Cookie_Accepted(bool preference)
        {
            var cookieSerialized = SerializeCookie(true, false, preference);
            SetUpCookie(cookieSerialized);
            var service = CreateStrut();
            service.SetPreference(preference);

            var cookie = service.GetCookie();
            Assert.IsType<DfeCookie>(cookie);
            Assert.Equal(preference, cookie.HasApproved);
            Assert.False(cookie.IsRejected);
            Assert.True(cookie.IsVisible);
        }

        [Fact]
        public void GetCookie_Returns_Cookie_When_Cookie_Exists()
        {
            var cookieSerialized = SerializeCookie(true, false, true);
            var requestCookiesMock = Substitute.For<IRequestCookieCollection>();
            requestCookiesMock["cookies_preferences_set"].Returns(cookieSerialized);

            mockHttp.HttpContext.Request.Cookies.Returns(requestCookiesMock);

            var service = CreateStrut();
            var cookie = service.GetCookie();
            Assert.IsType<DfeCookie>(cookie);
            Assert.True(cookie.HasApproved);
            Assert.False(cookie.IsRejected);
            Assert.True(cookie.IsVisible);
        }

        //TODO - Find out why DefaultHttpContext not working
        //[Fact]
        //public void GetCookie_Returns_Cookie_When_Cookie_Does_Not_Exists()
        //{
        //    var http = new DefaultHttpContext();
        //    mockHttp.Setup(x => x.HttpContext).Returns(http);
        //    var service = CreateStrut();
        //    var cookie = service.GetCookie();
        //    Assert.False(cookie.HasApproved);
        //    Assert.False(cookie.IsRejected);
        //    Assert.True(cookie.IsVisible);
        //}

        private void SetUpCookie(string cookieValue)
        {
            var requestCookiesMock = Substitute.For<IRequestCookieCollection>();
            requestCookiesMock["cookies_preferences_set"].Returns(cookieValue);
            var responseCookiesMock = Substitute.For<IResponseCookies>();
            responseCookiesMock.Delete("cookies_preferences_set");

            mockHttp.HttpContext.Request.Cookies.Returns(requestCookiesMock);
            mockHttp.HttpContext.Response.Cookies.Returns(responseCookiesMock);
        }

        private static string SerializeCookie(bool visibility, bool rejected, bool hasApproved)
        {
            var cookie = new DfeCookie { IsVisible = visibility, IsRejected = rejected, HasApproved = hasApproved };
            return JsonSerializer.Serialize(cookie);
        }
    }

}
