using System.Text.Json;
using Dfe.PlanTech.Application.Cookie.Service;
using Dfe.PlanTech.Domain.Cookie;
using Microsoft.AspNetCore.Http;
using NSubstitute;


namespace Dfe.PlanTech.Application.UnitTests.Cookie.Service;

public class CookieServiceTests
{
    private readonly ICookiesCleaner _cookiesCleaner = Substitute.For<ICookiesCleaner>();
    readonly IHttpContextAccessor Http = Substitute.For<IHttpContextAccessor>();

    private CookieService CreateStrut()
    {
        return new CookieService(Http, _cookiesCleaner);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetVisibility_Sets_Cookie_Visbility(bool visibility)
    {
        var cookieSerialized = SerializeCookie(visibility: visibility, userAcceptsCookies: null);
        SetUpCookie(cookieSerialized);

        var service = CreateStrut();
        service.SetVisibility(visibility);

        var cookie = service.GetCookie();
        Assert.IsType<DfeCookieModel>(cookie);
        Assert.Equal(visibility, cookie.IsVisible);
        Assert.Null(cookie.UserAcceptsCookies);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetCookieAcceptance_Sets_Cookie_Acceptance(bool userAcceptsCookies)
    {
        var cookieSerialized = SerializeCookie(visibility: true, userAcceptsCookies);
        SetUpCookie(cookieSerialized);
        var service = CreateStrut();
        service.SetCookieAcceptance(userAcceptsCookies);

        var cookie = service.GetCookie();
        Assert.IsType<DfeCookieModel>(cookie);
        Assert.Equal(userAcceptsCookies, cookie.UserAcceptsCookies);
        Assert.True(cookie.IsVisible);
        Assert.True(cookie.UserPreferencesSet);
        Assert.Equal(userAcceptsCookies, cookie.UseCookies);
    }

    [Fact]
    public void GetCookie_Returns_Cookie_When_Cookie_Exists()
    {
        var cookieSerialized = SerializeCookie(visibility: true, userAcceptsCookies: true);
        var requestCookiesSubstitute = Substitute.For<IRequestCookieCollection>();
        requestCookiesSubstitute[CookieService.Cookie_Key].Returns(cookieSerialized);

        Http.HttpContext!.Request.Cookies.Returns(requestCookiesSubstitute);

        var service = CreateStrut();
        var cookie = service.GetCookie();
        Assert.IsType<DfeCookieModel>(cookie);
        Assert.True(cookie.UserAcceptsCookies);
        Assert.True(cookie.IsVisible);
    }

    [Fact]
    public void UseCookies_Returns_False_When_UserAcceptsCookies_Null()
    {
        var cookieSerialized = SerializeCookie(visibility: true, userAcceptsCookies: null);
        var requestCookiesSubstitute = Substitute.For<IRequestCookieCollection>();
        requestCookiesSubstitute[CookieService.Cookie_Key].Returns(cookieSerialized);

        Http.HttpContext!.Request.Cookies.Returns(requestCookiesSubstitute);

        var service = CreateStrut();
        var cookie = service.GetCookie();
        Assert.IsType<DfeCookieModel>(cookie);
        Assert.Null(cookie.UserAcceptsCookies);
        Assert.True(cookie.IsVisible);
        Assert.False(cookie.UseCookies);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public void UserPreferencesSet_Returns_UserAcceptsCookies_Status(bool? userAcceptsCookies, bool expectedResult)
    {
        var cookieSerialized = SerializeCookie(visibility: true, userAcceptsCookies: userAcceptsCookies);
        var requestCookiesSubstitute = Substitute.For<IRequestCookieCollection>();
        requestCookiesSubstitute[CookieService.Cookie_Key].Returns(cookieSerialized);

        Http.HttpContext!.Request.Cookies.Returns(requestCookiesSubstitute);

        var service = CreateStrut();
        var cookie = service.GetCookie();
        Assert.IsType<DfeCookieModel>(cookie);
        Assert.Equal(userAcceptsCookies, cookie.UserAcceptsCookies);
        Assert.True(cookie.IsVisible);
        Assert.Equal(expectedResult, cookie.UserPreferencesSet);
    }

    private void SetUpCookie(string cookieValue)
    {
        var requestCookiesSubstitute = Substitute.For<IRequestCookieCollection>();
        requestCookiesSubstitute[CookieService.Cookie_Key].Returns(cookieValue);
        var responseCookiesSubstitute = Substitute.For<IResponseCookies>();
        responseCookiesSubstitute.Delete(CookieService.Cookie_Key);

        Http.HttpContext!.Request.Cookies.Returns(requestCookiesSubstitute);
        Http.HttpContext.Response.Cookies.Returns(responseCookiesSubstitute);
    }

    private static string SerializeCookie(bool visibility, bool? userAcceptsCookies)
    {
        var cookie = new DfeCookieModel { IsVisible = visibility, UserAcceptsCookies = userAcceptsCookies };
        return JsonSerializer.Serialize(cookie);
    }
}
