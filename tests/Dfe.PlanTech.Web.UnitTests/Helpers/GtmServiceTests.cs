using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Helpers;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class GtmServiceTests
{
    public GtmServiceTests()
    {
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("id-one", "siteVerificationId-one")]
    [InlineData("text", "otherText")]
    [InlineData("example", "other example")]
    public void Should_Set_Values(string id, string siteVerificationId)
    {
        var config = new GtmConfiguration
        {
            Id = id,
            SiteVerificationId = siteVerificationId
        };

        var cookieService = Substitute.For<ICookieService>();

        var gtmService = new GtmServiceConfiguration(config, cookieService);

        Assert.Equal(id, config.Id);
        Assert.Equal(siteVerificationId, config.SiteVerificationId);

        Assert.Equal(config, gtmService.Config);
        Assert.Equal(cookieService, gtmService.Cookies);
    }
}
