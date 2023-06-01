using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class ComponentViewsFactoryTests
{
    [Fact]
    public void Should_ReturnStringWithTypeName_When_PassedObject()
    {
        var header = new Header();

        var viewName = ComponentViewsFactory.GetViewForType(header);

        Assert.Contains(header.GetType().Name, viewName);
        Assert.Contains("Components", viewName);
    }
}
