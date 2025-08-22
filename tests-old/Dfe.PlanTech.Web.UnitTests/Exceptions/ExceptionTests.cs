using Dfe.PlanTech.Core.Exceptions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Exceptions;

public class ExceptionTests
{
    [Theory]
    [InlineData(typeof(ExceptionTests))]
    [InlineData(typeof(MissingServiceException))]
    [InlineData(typeof(string))]
    public void MissingServiceException_Should_Set_Message_From_Type(Type type)
    {
        var message = $"Missing service {type.Name}";
        var exception = new MissingServiceException(type);

        Assert.Equal(message, exception.Message);
    }

    [Theory]
    [InlineData("Exception one")]
    [InlineData("Exception two")]
    [InlineData("")]
    public void MissingServiceException_Should_Set_Message_From_String(string message)
    {
        var exception = new MissingServiceException(message);

        Assert.Equal(message, exception.Message);
    }
}
