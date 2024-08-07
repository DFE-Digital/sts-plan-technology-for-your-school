using Dfe.PlanTech.Domain.Exceptions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Exceptions;

public class ExceptionTests
{
  [Theory]
  [InlineData(typeof(ExceptionTests))]
  [InlineData(typeof(MissingServiceException))]
  [InlineData(typeof(string))]
  public void MissingServiceException_Should_Set_Message(Type type)
  {
    var message = $"Missing service {type.Name}";
    var exception = new MissingServiceException(message);

    Assert.Equal(message, exception.Message);
  }
}