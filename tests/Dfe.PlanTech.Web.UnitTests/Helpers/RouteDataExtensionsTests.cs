using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Routing;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class RouteDataExtensionsTests
{
    [Theory]
    [InlineData("HomeController", "Home")]
    [InlineData("AccountController", "Account")]
    [InlineData("Foo", "Foo")] // no "Controller" suffix
    public void GetControllerNameSlug_Strips_Controller_Suffix(string input, string expected)
    {
        var result = input.GetControllerNameSlug();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetTitleForPage_Returns_Default_When_No_Slug()
    {
        var rd = new RouteData();
        var result = rd.GetTitleForPage();

        Assert.Equal(RouteDataExtensions.DefaultPageTitle, result);
    }

    [Fact]
    public void GetTitleForPage_Uses_SectionSlug_Key_When_Present()
    {
        var rd = new RouteData();
        rd.Values[RouteDataExtensions.SectionSlugKey] = "cyber-security";

        var result = rd.GetTitleForPage();

        // "Cyber security"
        Assert.Equal("Cyber security", result);
    }

    [Fact]
    public void GetTitleForPage_Picks_Other_String_Value_When_No_SectionSlug()
    {
        var rd = new RouteData();
        rd.Values["random"] = "cloud-computing";

        var result = rd.GetTitleForPage();

        Assert.Equal("Cloud computing", result);
    }

    [Fact]
    public void GetTitleForPage_Skips_Empty_Or_Slash_Or_Numeric_Values()
    {
        var rd = new RouteData();
        rd.Values["a"] = "/";
        rd.Values["b"] = "123";
        rd.Values["c"] = "";
        rd.Values["sectionSlug"] = "data-protection";

        var result = rd.GetTitleForPage();

        Assert.Equal("Data protection", result);
    }

    [Fact]
    public void GetTitleForPage_Adds_Spaces_Before_Capitals()
    {
        var rd = new RouteData();
        rd.Values["sectionSlug"] = "dataProtectionPolicy";

        var result = rd.GetTitleForPage();

        Assert.Equal("Data Protection Policy", result);
    }

    [Fact]
    public void GetTitleForPage_First_Letter_Is_Capitalised()
    {
        var rd = new RouteData();
        rd.Values["sectionSlug"] = "information-management";

        var result = rd.GetTitleForPage();

        Assert.Equal("Information management", result);
    }

    [Fact]
    public void GetTitleForPage_Orders_By_Key_Descending_When_Multiple_Valid()
    {
        var rd = new RouteData();
        rd.Values["a"] = "networking";
        rd.Values["z"] = "cyber"; // higher key alphabetically, should win

        var result = rd.GetTitleForPage();

        Assert.Equal("Cyber", result);
    }
}
