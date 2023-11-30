using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using AutoMapper;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions
{
  public class TestingJsonParser
  {
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    private List<Type> _allTypes;

    public TestingJsonParser(ILoggerFactory loggerFactory, IMapper mapper)
    {
      _logger = loggerFactory.CreateLogger<TestingJsonParser>();
      _mapper = mapper;

      _allTypes = GetAllTypes();
    }

    private List<Type> GetAllTypes() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembley => assembley.GetTypes()).ToList();

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

        if (node != null)
        {
          await response.WriteAsJsonAsync(node);
        }

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

      var sys = asJson["sys"];

      if (sys == null)
      {
        _logger.LogError("Missing sys");
        jsonNode = null;
        return false;
      }

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
          fields[FirstCharToUpperAsSpan(field.Key)] = JsonNode.Parse(child.Value!.ToJsonString());
        }
      }

      fields["ContentfulId"] = JsonNode.Parse(asJson["sys"]!["id"].ToJsonString());

      var contentType = $"{FirstCharToUpperAsSpan(sys["contentType"]["sys"]["id"].ToString())}DbEntity";
      fields["ContentType"] = contentType;

      jsonNode = fields;

      Type? contentTypeType = _allTypes.Find(type => type.Name == contentType);

      if (contentTypeType == null)
      {
        throw new KeyNotFoundException($"Could not find matching type for {contentTypeType}");
      }

      var content = Activator.CreateInstance(contentTypeType);

      var mapped = _mapper.Map(jsonNode, content!, jsonNode.GetType(), content!.GetType());

      _logger.LogInformation(mapped.ToString());
      return true;
    }

    public string FirstCharToUpperAsSpan(string input)
    {
      if (string.IsNullOrEmpty(input))
      {
        return string.Empty;
      }
      Span<char> destination = stackalloc char[1];
      input.AsSpan(0, 1).ToUpperInvariant(destination);
      return $"{destination}{input.AsSpan(1)}";
    }

  }
}
