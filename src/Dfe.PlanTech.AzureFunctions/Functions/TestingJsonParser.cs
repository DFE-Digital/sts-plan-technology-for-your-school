using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AutoMapper;
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
    private readonly IMapper _mapper;

    private readonly List<Type> _allTypes;

    private readonly CmsDbContext _db;

    private static JsonSerializerOptions _jsonOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TestingJsonParser(ILoggerFactory loggerFactory, IMapper mapper, CmsDbContext db)
    {
      _logger = loggerFactory.CreateLogger<TestingJsonParser>();
      _mapper = mapper;

      _allTypes = GetAllTypes();
      _db = db;
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

        var responseType = HttpStatusCode.OK;

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
      var success = SerialiseBody(body, out CmsWebHookPayload? payload);

      if (!success || payload == null || payload.Sys == null) return false;

      var fields = new Dictionary<string, object>
      {
        ["Id"] = payload.Sys.Id
      };

      foreach (var field in payload!.Fields)
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
          if (child.Value == null)
          {
            _logger.LogTrace($"Null value for {child.Key}");
            continue;
          }

          fields[FirstCharToUpperAsSpan(field.Key)] = TrySerialiseAsLinkEntry(child.Value, out CmsWebHookSystemDetailsInner? sys) ? sys! : child.Value;
        }
      }

      fields["Id"] = payload.Sys.Id;

      var contentType = $"{FirstCharToUpperAsSpan(payload.Sys.ContentType.Sys.Id)}DbEntity";

      object? mapped = MapObjectToDbEntity(fields, contentType);

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
