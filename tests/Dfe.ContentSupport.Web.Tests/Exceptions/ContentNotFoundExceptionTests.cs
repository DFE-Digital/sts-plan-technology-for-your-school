using Dfe.ContentSupport.Web.Exceptions;

namespace Dfe.ContentSupport.Web.Tests.Exceptions;

public class ContentNotFoundExceptionTests
{
    [Fact]
    public void Exception_Generates_Expected()
    {
        const string message = "dummy";

        var ex = new ContentNotFoundException(message);

        ex.Should().BeOfType<ContentNotFoundException>();
        ex.Should().BeAssignableTo<Exception>();
        ex.Message.Should().BeEquivalentTo(message);
    }
}