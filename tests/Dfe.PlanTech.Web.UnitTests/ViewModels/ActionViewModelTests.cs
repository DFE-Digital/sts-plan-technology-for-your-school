using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Routing;

namespace Dfe.PlanTech.Web.UnitTests.ViewModels;

public class ActionViewModelTests
{
    [Fact]
    public void DefaultConstructor_AllowsDeserializationStyleCreation()
    {
        var sut = new ActionViewModel
        {
            ActionName = "Index",
            ControllerName = "Home",
            LinkText = "Go home",
        };

        Assert.Equal("Index", sut.ActionName);
        Assert.Equal("Home", sut.ControllerName);
        Assert.Equal("Go home", sut.LinkText);
        Assert.Null(sut.RouteValues);
    }

    [Theory]
    [InlineData(null!, "Home", "LinkText")]
    [InlineData("   ", "Home", "LinkText")]
    [InlineData("Index", null!, "LinkText")]
    [InlineData("Index", "   ", "LinkText")]
    [InlineData("Index", "Home", null!)]
    [InlineData("Index", "Home", "   ")]
    public void Constructor_WhenParameterIsInvalid_ThrowsInvalidDataException(
        string? actionName,
        string? controllerName,
        string? linkText
    )
    {
        var act = () => new ActionViewModel(actionName!, controllerName!, linkText!);

        var exception = Assert.Throws<InvalidDataException>(act);

        Assert.Equal(
            "ActionViewModel must be provided with a controller name, action name, and link text",
            exception.Message
        );
    }

    [Fact]
    public void Constructor_WhenControllerNameContainsController_OnlyStripsControllerSuffix()
    {
        var sut = new ActionViewModel("Index", "SomethingControllerBaseController", "Back");

        Assert.Equal("Index", sut.ActionName);
        Assert.Equal("SomethingControllerBase", sut.ControllerName);
        Assert.Equal("Back", sut.LinkText);
    }

    [Fact]
    public void Constructor_WhenControllerNameEndsWithController_StripsControllerSuffix()
    {
        var sut = new ActionViewModel("Index", "PagesController", "Back");

        Assert.Equal("Index", sut.ActionName);
        Assert.Equal("Pages", sut.ControllerName);
        Assert.Equal("Back", sut.LinkText);
    }

    [Fact]
    public void Constructor_WhenControllerNameDoesNotEndWithController_LeavesItUnchanged()
    {
        var sut = new ActionViewModel("Index", "Pages", "Back");

        Assert.Equal("Pages", sut.ControllerName);
    }

    [Fact]
    public void Constructor_WhenRouteValuesIsDictionary_SetsRouteValuesDirectly()
    {
        var routeValues = new Dictionary<string, object>
        {
            ["route"] = "digital-leadership",
            ["page"] = 2,
        };

        var sut = new ActionViewModel("Index", "Pages", "Back", routeValues);

        Assert.NotNull(sut.RouteValues);
        Assert.Equal(2, sut.RouteValues!.Count);
        Assert.Equal("digital-leadership", sut.RouteValues["route"]);
        Assert.Equal(2, sut.RouteValues["page"]);
        Assert.Same(routeValues, sut.RouteValues);
    }

    [Fact]
    public void Constructor_WhenRouteValuesIsRouteValueDictionary_ConvertsToDictionary()
    {
        var routeValues = new RouteValueDictionary
        {
            ["route"] = "digital-leadership",
            ["page"] = 2,
            ["filter"] = null,
        };

        var sut = new ActionViewModel("Index", "Pages", "Back", routeValues);

        Assert.NotNull(sut.RouteValues);
        Assert.Equal("digital-leadership", sut.RouteValues!["route"]);
        Assert.Equal(2, sut.RouteValues["page"]);
        Assert.Equal(string.Empty, sut.RouteValues["filter"]);
    }

    [Fact]
    public void Constructor_WhenRouteValuesIsAnonymousObject_ConvertsToDictionary()
    {
        var sut = new ActionViewModel(
            "Index",
            "Pages",
            "Back",
            new { route = "digital-leadership", page = 2 }
        );

        Assert.NotNull(sut.RouteValues);
        Assert.Equal("digital-leadership", sut.RouteValues!["route"]);
        Assert.Equal(2, sut.RouteValues["page"]);
    }

    [Fact]
    public void Constructor_WhenRouteValuesIsSimpleType_String_IgnoresIt()
    {
        var sut = new ActionViewModel("Index", "Pages", "Back", "not-a-route-object");

        Assert.Null(sut.RouteValues);
    }

    [Fact]
    public void Constructor_WhenRouteValuesIsSimpleType_Int_IgnoresIt()
    {
        var sut = new ActionViewModel("Index", "Pages", "Back", 123);

        Assert.Null(sut.RouteValues);
    }

    [Fact]
    public void Constructor_WhenRouteValuesIsNull_LeavesRouteValuesNull()
    {
        var sut = new ActionViewModel("Index", "Pages", "Back", null);

        Assert.Null(sut.RouteValues);
    }

    [Fact]
    public void Constructor_WhenRouteValuesConversionFails_SetsRouteValuesToNull()
    {
        var badRouteValues = new ObjectWithThrowingProperty();

        var sut = new ActionViewModel("Index", "Pages", "Back", badRouteValues);

        Assert.Null(sut.RouteValues);
    }

    private sealed class ObjectWithThrowingProperty
    {
        public string ValidProperty => "value";

        public string ExplodingProperty => throw new InvalidOperationException("Nope");
    }
}
