using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;
public class RouteDataExtensionsTests
{
    [Fact]
    public void GetTitleForPage_WhenSectionSlugIsEmpty_ReturnsDefaultPageTitle()
    {
        var routeData = new RouteData();

        var result = routeData.GetTitleForPage();

        Assert.Equal(RouteDataExtensions.DefaultPageTitle, result);
    }

    [Fact]
    public void GetTitleForPage_Returns_SectionSlug()
    {
        var routeData = new RouteData();
        routeData.Values.Add("sectionSlug", "broadband-connection");

        var result = routeData.GetTitleForPage();

        Assert.Equal("Broadband connection", result);
    }

    [Fact]
    public void GetTitleForPage_Ignores_ForwardSlashes()
    {
        var routeData = new RouteData();
        routeData.Values.Add("sectionSlug", "broadband-connection");
        routeData.Values.Add("route", "/");

        var result = routeData.GetTitleForPage();

        Assert.Equal("Broadband connection", result);
    }

    [Fact]
    public void GetTitleForPage_Ignores_Numbers()
    {
        var routeData = new RouteData();
        routeData.Values.Add("sectionSlug", "broadband-connection");
        routeData.Values.Add("route", "abcd-1234");

        var result = routeData.GetTitleForPage();

        Assert.Equal("Broadband connection", result);
    }
}
