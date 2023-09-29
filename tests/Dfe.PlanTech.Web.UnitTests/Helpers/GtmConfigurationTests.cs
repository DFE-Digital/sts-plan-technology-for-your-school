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

    public readonly ICookieService CookieService;
    public readonly IConfiguration Configuration;

    public GtmConfigurationTests()
    {
        var inMemorySettings = new Dictionary<string, string?> {
      {GTM_BODY_KEY, GTM_BODY_VALUE},
      {GTM_HEAD_KEY, GTM_HEAD_VALUE},
    };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        CookieService = Substitute.For<ICookieService>();
    }

    [Fact]
    public void Should_Return_BodyAndHead_When_Cookies_Accepted()
    {
        CookieService.GetCookie().Returns((_) => new DfeCookie()
        {
            HasApproved = true
        });

        var gtmConfiguration = new GtmConfiguration(CookieService, Configuration);

        Assert.Equal(GTM_BODY_VALUE, gtmConfiguration.Body);
        Assert.Equal(GTM_HEAD_VALUE, gtmConfiguration.Head);
    }

    [Fact]
    public void Should_Return_Empty_When_Cookies_Not_Accepted()
    {
        CookieService.GetCookie().Returns((_) => new DfeCookie()
        {
            HasApproved = false
        });

        var gtmConfiguration = new GtmConfiguration(CookieService, Configuration);

        Assert.Empty(gtmConfiguration.Body);
        Assert.Empty(gtmConfiguration.Head);
    }
}