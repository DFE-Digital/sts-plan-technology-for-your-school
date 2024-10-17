using Dfe.ContentSupport.Web.Extensions;

namespace Dfe.ContentSupport.Web.Tests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToLongString_Formats_Returns_Expected()
    {
        var testValue = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Local);
        const string expected = "01 February 2024";

        var result = testValue.ToLongString();

        result.Should().Be(expected);
    }
}
