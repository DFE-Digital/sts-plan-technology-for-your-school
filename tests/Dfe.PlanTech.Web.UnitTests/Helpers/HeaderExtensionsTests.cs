using System.ComponentModel;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.Helpers;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class HeaderExtensionsTests
{
    private static ComponentHeaderEntry Header(HeaderSize size) => new ComponentHeaderEntry { Size = size };

    [Theory]
    [InlineData(HeaderSize.Small, HeaderExtensions.SMALL)]
    [InlineData(HeaderSize.Medium, HeaderExtensions.MEDIUM)]
    [InlineData(HeaderSize.Large, HeaderExtensions.LARGE)]
    [InlineData(HeaderSize.ExtraLarge, HeaderExtensions.EXTRALARGE)]
    public void GetClassForSize_Returns_Expected_Class(HeaderSize size, string expected)
    {
        var header = Header(size);

        var result = header.GetClassForSize();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetClassForSize_Throws_For_Unknown_Enum_Value()
    {
        var invalid = (HeaderSize)999;
        var header = Header(invalid);

        var ex = Assert.Throws<InvalidEnumArgumentException>(() => header.GetClassForSize());
        Assert.Contains("Could not find header size", ex.Message);
    }
}
