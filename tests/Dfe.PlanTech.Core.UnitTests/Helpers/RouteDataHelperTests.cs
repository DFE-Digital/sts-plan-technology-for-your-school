using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.UnitTests.Helpers;

public class RouteDataHelperTests
{
    [Theory]
    [InlineData("Pages", "Pages")]
    [InlineData("PagesController", "Pages")]
    [InlineData("BaseControllerThingController", "BaseControllerThing")]
    public void GetControllerNameSlug_Returns_String_Before_Controller(
        string controllerName,
        string expectedValue
    )
    {
        var newControllerName = controllerName.GetControllerNameSlug();

        Assert.Equal(expectedValue, newControllerName);
    }
}
