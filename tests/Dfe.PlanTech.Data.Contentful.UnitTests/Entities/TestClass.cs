using Dfe.PlanTech.Core.Attributes;

namespace Dfe.PlanTech.Data.Contentful.UnitTests.Entities;

[ContentfulType("testClass")]
public class TestClass
{
    public TestClass() { }

    public TestClass(string id)
    {
        Id = id;
    }

    public string? Id { get; set; }
}
