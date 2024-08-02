using Dfe.PlanTech.Domain.Cookie;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class GtmConfigurationTests
{
    private const string GTM_BODY_KEY = "GTM:Body";
    private const string GTM_BODY_VALUE = "<noscript>Google tag manager body</noscript>";

    private const string GTM_HEAD_KEY = "GTM:Head";
    private const string GTM_HEAD_VALUE = "<noscript>Google tag manager Head</noscript>";

    private const string GTM_ANALYTICS_KEY = "GTM:Analytics";
    private const string GTM_ANALYTICS_VALUE = "<meta name=\"google-site-verification\" content=\"TEST\" />";

    public readonly ICookieService CookieService;
    public readonly IConfiguration Configuration;

    public GtmConfigurationTests()
    {
        var inMemorySettings = new Dictionary<string, string?> {
      {GTM_BODY_KEY, GTM_BODY_VALUE},
      {GTM_HEAD_KEY, GTM_HEAD_VALUE},
      {GTM_ANALYTICS_KEY, GTM_ANALYTICS_VALUE},
    };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        CookieService = Substitute.For<ICookieService>();
    }

    [Fact]
    public void Should_Return_BodyAndHead_When_Cookies_Accepted()
    {
        var cookie = CreateCookie(true);
        CookieService.Cookie.Returns(cookie);

        var gtmConfiguration = new GtmConfiguration(CookieService, Configuration);

        Assert.Equal(GTM_BODY_VALUE, gtmConfiguration.Body);
        Assert.Equal(GTM_HEAD_VALUE, gtmConfiguration.Head);
        Assert.Equal(GTM_ANALYTICS_VALUE, gtmConfiguration.Analytics);
    }

    [Fact]
    public void Should_Return_Empty_When_Cookies_Not_Accepted()
    {
        var cookie = CreateCookie(false);
        CookieService.Cookie.Returns(cookie);

        var gtmConfiguration = new GtmConfiguration(CookieService, Configuration);

        Assert.Empty(gtmConfiguration.Body);
        Assert.Empty(gtmConfiguration.Head);
        Assert.Equal(GTM_ANALYTICS_VALUE, gtmConfiguration.Analytics);
    }

    [Fact]
    public void Should_Return_Empty_When_Cookie_Preferences_Not_Set()
    {
        var cookie = CreateCookie(null);
        CookieService.Cookie.Returns(cookie);

        var gtmConfiguration = new GtmConfiguration(CookieService, Configuration);

        Assert.Empty(gtmConfiguration.Body);
        Assert.NotEmpty(gtmConfiguration.Head);
        Assert.Equal(GTM_ANALYTICS_VALUE, gtmConfiguration.Analytics);
    }

    private static DfeCookie CreateCookie(bool? userAcceptsCookie) => new() { UserAcceptsCookies = userAcceptsCookie };
}
