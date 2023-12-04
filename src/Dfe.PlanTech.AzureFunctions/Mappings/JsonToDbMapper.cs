using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public abstract class JsonToDbMapper<TEntity> : JsonToDbMapper
where TEntity : ContentComponentDbEntity, new()
{
  public JsonToDbMapper(ILogger<JsonToDbMapper<TEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(typeof(TEntity), logger, jsonSerialiserOptions)
  {
  }

  public TEntity ToEntity(CmsWebHookPayload payload)
  {
    var values = GetEntityValuesDictionary(payload);
    values = PerformAdditionalMapping(values);

    var asJson = JsonSerializer.Serialize(values);
    var serialised = JsonSerializer.Deserialize<TEntity>(asJson, JsonOptions);

    return serialised ?? throw new Exception("Null value");

  }

  public override ContentComponentDbEntity MapEntity(CmsWebHookPayload payload) => ToEntity(payload);

  public abstract Dictionary<string, object> PerformAdditionalMapping(Dictionary<string, object> values);
}


public abstract class JsonToDbMapper
{
  protected readonly ILogger Logger;
  protected readonly JsonSerializerOptions JsonOptions;

  private readonly Type _entityType;
  public JsonToDbMapper(Type entityType, ILogger logger, JsonSerializerOptions jsonSerialiserOptions)
  {
    _entityType = entityType;
    Logger = logger;
    JsonOptions = jsonSerialiserOptions;
  }

  public bool AcceptsContentType(string contentType)
    => _entityType.Name.ToLower().Contains(contentType);

  public abstract ContentComponentDbEntity MapEntity(CmsWebHookPayload payload);

  protected Dictionary<string, object> GetEntityValuesDictionary(CmsWebHookPayload payload)
  => GetEntityValues(payload).Where(kvp => kvp.HasValue)
                                        .Select(kvp => kvp!.Value)
                                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

  protected virtual IEnumerable<KeyValuePair<string, object>?> GetEntityValues(CmsWebHookPayload payload)
  {
    yield return new KeyValuePair<string, object>("id", payload.Sys.Id);

    foreach (var field in payload.Fields.SelectMany(GetValuesFromFields))
    {
      yield return field;
    }
  }

  protected virtual IEnumerable<KeyValuePair<string, object>?> GetValuesFromFields(KeyValuePair<string, JsonNode> field)
  {
    if (field.Value == null)
    {
      Logger.LogError("No value for {field}", field);
      yield return null;
    }

    var fieldChildren = field.Value!.AsObject();

    if (fieldChildren.Count > 1)
    {
      Logger.LogError("Expected only one language - received {count}", fieldChildren.Count);
      yield return null;
    }

    foreach (var child in fieldChildren)
    {
      if (child.Value == null)
      {
        Logger.LogTrace($"Null value for {child.Key}");
        yield return null;
      }

      yield return TrySerialiseAsLinkEntry(child.Value!, out CmsWebHookSystemDetailsInner? sys) && sys != null ?
                      new KeyValuePair<string, object>(field.Key, sys) : new KeyValuePair<string, object>(field.Key, child.Value!);
    }
  }

  protected virtual bool TrySerialiseAsLinkEntry(JsonNode node, out CmsWebHookSystemDetailsInner? sys)
  {
    if (node is not JsonObject jsonObject)
    {
      sys = null;
      return false;
    }

    var container = JsonSerializer.Deserialize<CmsWebHookSystemDetailsInnerContainer>(jsonObject, JsonOptions);

    if (container?.Sys == null)
    {
      sys = null;
      return false;
    }

    sys = container.Sys;

    return !string.IsNullOrEmpty(sys.Id) && !string.IsNullOrEmpty(sys.LinkType) && !string.IsNullOrEmpty(sys.Type);
  }
}