using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions
{
  public class TestingJsonParser
  {
    private readonly ILogger _logger;

    public TestingJsonParser(ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<TestingJsonParser>();
    }

    [Function("TestingJsonParser")]
    public async Task<HttpResponseData> WebhookReceiver([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
      var stream = new StreamReader(req.Body);
      var body = stream.ReadToEnd();

      try
      {
        var processed = TryNormaliseJson(body, out JsonNode? node);

        var responseType = processed ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

        var response = req.CreateResponse(responseType);

        if (node != null) await response.WriteAsJsonAsync(node);

        return response;
      }
      catch (Exception ex)
      {
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);

        var sb = new StringBuilder();

        var currentException = ex;

        while (currentException != null)
        {
          sb.AppendLine(currentException.Message);
          sb.AppendLine(currentException.StackTrace);
          currentException = ex.InnerException;
        }
        await response.WriteStringAsync(sb.ToString());

        return response;
      }
    }

    private bool TryNormaliseJson(string text, out JsonNode? jsonNode)
    {
      var asJson = JsonNode.Parse(text);

      if (asJson == null)
      {
        _logger.LogError("Invalid JSON");
        jsonNode = null;
        return false;
      }

      var fieldsNode = asJson["fields"];

      if (fieldsNode == null)
      {
        _logger.LogError("Missing fields");
        jsonNode = null;
        return false;
      }

      var copy = JsonNode.Parse(text);
      var fields = new JsonObject();

      foreach (var field in fieldsNode.AsObject())
      {
        if (field.Value == null)
        {
          _logger.LogError("No value for {field}", field);
          continue;
        }

        var fieldChildren = field.Value.AsObject();

        if (fieldChildren.Count > 1)
        {
          _logger.LogError("Expected only one language - received {count}", fieldChildren.Count);
          continue;
        }

        foreach (var child in fieldChildren)
        {
          fields[field.Key] = JsonNode.Parse(child.Value.ToJsonString());
        }
      }

      copy!["fields"] = fields;
      jsonNode = copy;

      return true;
    }
  }
}
