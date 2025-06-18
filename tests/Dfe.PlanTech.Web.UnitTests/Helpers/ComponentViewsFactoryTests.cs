using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class ComponentViewsFactoryTests
{
    [Fact]
    public void Should_ReturnStringWithTypeName_When_PassedObject()
    {
        var header = new Header();

        var factory = new ComponentViewsHelper(new NullLogger<ComponentViewsHelper>());
        var success = factory.TryGetViewForType(header, out string? viewPath);

        Assert.True(success);
        Assert.Contains(header.GetType().Name, viewPath);
        Assert.Contains("Components", viewPath);
    }

    [Fact]
    public void TryGetViewForType_Should_Return_False_When_No_View_Found()
    {
        var testClass = new NotARealClass();

        var factory = new ComponentViewsHelper(new NullLogger<ComponentViewsHelper>());
        var success = factory.TryGetViewForType(testClass, out string? viewPath);

        Assert.False(success);
        Assert.Null(viewPath);
    }
}

public class NotARealClass
{

}
