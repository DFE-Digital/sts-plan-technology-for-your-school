using System.Net;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class BaseFunctionTests
{

    private readonly BaseFunction _baseFunction;
    private readonly ILogger _logger;

    public BaseFunctionTests()
    {
        _logger = Substitute.For<ILogger>();
        _baseFunction = new BaseFunction(_logger);
    }

    [Fact]
    public void ReturnEmptyBodyError_Should_ReturnBadResponse_And_LogMessage()
    {
        var request = HttpHelpers.MockHttpRequest();

        var response = _baseFunction.ReturnEmptyBodyError(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var loggerResults = _logger.ReceivedWithAnyArgs(1);
        Assert.NotNull(loggerResults);
    }

    [Fact]
    public void ReturnServerErrorResponse_Should_LogError()
    {
        var request = HttpHelpers.MockHttpRequest();

        var errorMessage = "ERROR MESSAGE GOES HERE";
        var exception = new Exception(errorMessage);

        var response = _baseFunction.ReturnServerErrorResponse(request, exception);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var loggerResults = _logger.ReceivedWithAnyArgs(1);
        Assert.NotNull(loggerResults);
        var loggerArguments = _logger.ReceivedCalls().First().GetArguments();

        var formattableStrings = loggerArguments[2] as IEnumerable<KeyValuePair<string, object>>;
        Assert.NotNull(formattableStrings);
        Assert.NotEmpty(formattableStrings);

        var receivedErrorMessage = formattableStrings.Where(s => s.Key == "message").Select(s => s.Value).FirstOrDefault();

        Assert.NotNull(receivedErrorMessage);
        Assert.IsType<string>(receivedErrorMessage);
        Assert.Equal(errorMessage, receivedErrorMessage);
    }

    [Fact]
    public void ReturnOkResponse_Should_ReturnOkResponse()
    {
        var request = HttpHelpers.MockHttpRequest();

        var response = BaseFunction.ReturnOkResponse(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
