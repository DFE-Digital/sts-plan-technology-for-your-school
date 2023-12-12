using System.Net;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

  public static HttpRequestData MockHttpRequest()
  {
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
    var serviceProvider = serviceCollection.BuildServiceProvider();

    var context = Substitute.For<FunctionContext>();
    context.InstanceServices.Returns(serviceProvider);

    var request = Substitute.For<HttpRequestData>(context);

    request.CreateResponse().Returns((callInfo) =>
    {
      var response = Substitute.For<HttpResponseData>(context);
      response.Headers.Returns(new HttpHeadersCollection());
      response.Body.Returns(new MemoryStream());

      return response;
    });

    return request;
  }

  [Fact]
  public void ReturnEmptyBodyError_Should_ReturnBadResponse_And_LogMessage()
  {
    var request = MockHttpRequest();

    var response = _baseFunction.ReturnEmptyBodyError(request);

    Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
    _logger.ReceivedWithAnyArgs(1);
  }
}