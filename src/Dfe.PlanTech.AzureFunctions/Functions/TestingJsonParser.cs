using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions
{
  public class TestingJsonParser
  {
    private readonly ILogger _logger;

    private readonly List<Type> _allTypes;

    private readonly CmsDbContext _db;

    private static JsonSerializerOptions _jsonOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AnswerMapper _answerMapper;

    public TestingJsonParser(ILoggerFactory loggerFactory, CmsDbContext db, AnswerMapper answerMapper)
    {
      _logger = loggerFactory.CreateLogger<TestingJsonParser>();
      _allTypes = GetAllTypes();
      _db = db;

      _answerMapper = answerMapper;
    }

    private static List<Type> GetAllTypes() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembley => assembley.GetTypes()).ToList();

    [Function("TestingSerialisation")]
    public async Task<HttpResponseData> TestingSerialisation([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
      var stream = new StreamReader(req.Body);
      var body = stream.ReadToEnd();

      try
      {
        var processed = JsonSerializer.Deserialize<CmsWebHookPayload>(body, new JsonSerializerOptions()
        {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var answerDbEntity = _answerMapper.MapToEntity(processed!);

        return req.CreateResponse(HttpStatusCode.OK);
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

    [Function("TestingJsonParser")]
    public async Task<HttpResponseData> WebhookReceiver([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
      var stream = new StreamReader(req.Body);
      var body = stream.ReadToEnd();

      try
      {
        var processed = TryNormaliseJson(body);

        var responseType = processed ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

        var response = req.CreateResponse(responseType);

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

    private bool TryNormaliseJson(string body)
    {
      var processed = JsonSerializer.Deserialize<CmsWebHookPayload>(body, new JsonSerializerOptions()
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      });

      var mapped = _answerMapper.MapToEntity(processed!);

      if (mapped == null)
      {
        _logger.LogError($"Received null object back");
        return false;
      }

      if (mapped is not ContentComponentDbEntity contentComponent)
      {
        return false;
      }

      var existing = _db.Find(mapped.GetType(), contentComponent.Id);

      if (existing != null)
      {
        var properties = mapped.GetType().GetProperties();

        foreach (var property in properties)
        {
          property.SetValue(existing, property.GetValue(mapped));
        }
      }
      else
      {
        _db.Add(mapped);
      }

      _db.SaveChanges();

      return true;
    }

    public static bool TrySerialiseAsLinkEntry(JsonNode node, out CmsWebHookSystemDetailsInner? sys)
    {
      if (node is not JsonObject jsonObject)
      {
        sys = null;
        return false;
      }

      var container = JsonSerializer.Deserialize<CmsWebHookSystemDetailsInnerContainer>(jsonObject, _jsonOptions);

      if (container?.Sys == null)
      {
        sys = null;
        return false;
      }

      sys = container.Sys;

      return !string.IsNullOrEmpty(sys.Id) && !string.IsNullOrEmpty(sys.LinkType) && !string.IsNullOrEmpty(sys.Type);
    }

    private static bool SerialiseBody(string body, out CmsWebHookPayload? payload)
    {
      try
      {
        payload = JsonSerializer.Deserialize<CmsWebHookPayload>(body, _jsonOptions);

        return payload != null;
      }
      catch (Exception ex)
      {
        throw new Exception($"Error serialising body to {typeof(CmsWebHookPayload)}", ex);
      }
    }

    private object? MapObjectToDbEntity(Dictionary<string, object> fields, string contentType)
    {
      Type? contentTypeType = _allTypes.Find(type => type.Name == contentType);

      if (contentTypeType == null)
      {
        throw new KeyNotFoundException($"Could not find matching type for {contentTypeType}");
      }

      var asJson = JsonSerializer.Serialize(fields);
      var asConcreteType = JsonSerializer.Deserialize(asJson, contentTypeType);

      return asConcreteType;
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
