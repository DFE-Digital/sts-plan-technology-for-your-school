using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions;

public abstract class BaseFunction
{
  private readonly ILogger _logger;

  protected ILogger Logger => _logger;

  public BaseFunction(ILogger logger)
  {
    _logger = logger;
  }

  protected HttpResponseData ReturnEmptyBodyError(HttpRequestData req)
  {
    _logger.LogError("Received null body.");
    return req.CreateResponse(HttpStatusCode.BadRequest);
  }

  protected HttpResponseData ReturnServerErrorResponse(HttpRequestData req, Exception ex)
  {
    _logger.LogError("Error writing body to queue - {message} {stacktrace}", ex.Message, ex.StackTrace);
    return req.CreateResponse(HttpStatusCode.InternalServerError);
  }

  protected static HttpResponseData ReturnOkResponse(HttpRequestData req) => req.CreateResponse(HttpStatusCode.OK);
}