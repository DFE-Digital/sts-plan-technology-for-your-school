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

        var factory = new ComponentViewsFactory(new NullLogger<ComponentViewsFactory>());
        var success = factory.TryGetViewForType(header, out string? viewPath);

        Assert.True(success);
        Assert.Contains(header.GetType().Name, viewPath);
        Assert.Contains("Components", viewPath);
    }
}
