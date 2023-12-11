using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

/// <summary>
/// Maps a JSON payload to the given entity type
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class JsonToDbMapper<TEntity> : JsonToDbMapper
where TEntity : ContentComponentDbEntity, new()
{
  protected readonly TEntity MappedEntity = new();
  protected CmsWebHookPayload? Payload;

  public JsonToDbMapper(ILogger<JsonToDbMapper<TEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(typeof(TEntity), logger, jsonSerialiserOptions)
  {
  }

  public virtual TEntity ToEntity(CmsWebHookPayload payload)
  {
    Payload = payload;

    var values = GetEntityValuesDictionary(payload);
    values = PerformAdditionalMapping(values);

    var asJson = JsonSerializer.Serialize(values);
    var serialised = JsonSerializer.Deserialize<TEntity>(asJson, JsonOptions) ?? throw new NullReferenceException("Null returned");

    return serialised;
  }

  public override ContentComponentDbEntity MapEntity(CmsWebHookPayload payload) => ToEntity(payload);

  public abstract Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values);
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

  public virtual bool AcceptsContentType(string contentType)
    => _entityType.Name.Contains(contentType, StringComparison.InvariantCultureIgnoreCase);

  public abstract ContentComponentDbEntity MapEntity(CmsWebHookPayload payload);

  protected Dictionary<string, object?> GetEntityValuesDictionary(CmsWebHookPayload payload)
  => GetEntityValues(payload).Where(kvp => kvp.HasValue)
                                        .Select(kvp => kvp!.Value)
                                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

  /// <summary>
  /// Gets all values we care about from the payload
  /// </summary>
  /// <param name="payload"></param>
  /// <returns></returns>
  protected virtual IEnumerable<KeyValuePair<string, object?>?> GetEntityValues(CmsWebHookPayload payload)
  {
    yield return new KeyValuePair<string, object?>("id", payload.Sys.Id);

    foreach (var field in payload.Fields.SelectMany(GetValuesFromFields))
    {
      yield return field;
    }
  }

  /// <summary>
  /// Flattens the "fields" field (!); removes localisations etc.
  /// </summary>
  /// <param name="field"></param>
  /// <returns></returns>
  protected virtual IEnumerable<KeyValuePair<string, object?>?> GetValuesFromFields(KeyValuePair<string, JsonNode> field)
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
      var value = GetValue(child.Value!);

      yield return new KeyValuePair<string, object?>(field.Key, value);
    }
  }

  /// <summary>
  /// Get the value of this node in a format we care about
  /// </summary>
  /// <param name="node"></param>
  /// <returns></returns>
  private object? GetValue(JsonNode? node)
  {
    if (node == null)
    {
      return null;
    }

    if (node is JsonValue jsonValue)
    {
      return jsonValue;
    }

    if (node is JsonArray array)
    {
      return array.Select(GetValue).ToArray();
    }

    var asObject = node.AsObject();

    if (TrySerialiseAsLinkEntry(asObject, out CmsWebHookSystemDetailsInner? sys) && sys != null)
    {
      return sys.Id;
    }

    return node;
  }

  /// <summary>
  /// Tries to convert the JsonObject to the <see cref="CmsWebHookSystemDetailsInnerContainer"/> for easier processing  by other classes
  /// </summary>
  /// <param name="jsonObject"></param>
  /// <param name="sys"></param>
  /// <returns></returns>
  protected virtual bool TrySerialiseAsLinkEntry(JsonObject jsonObject, out CmsWebHookSystemDetailsInner? sys)
  {
    var container = JsonSerializer.Deserialize<CmsWebHookSystemDetailsInnerContainer>(jsonObject, JsonOptions);

    if (container?.Sys == null)
    {
      sys = null;
      return false;
    }

    sys = container.Sys;

    return !string.IsNullOrEmpty(sys.Id) && !string.IsNullOrEmpty(sys.LinkType) && !string.IsNullOrEmpty(sys.Type);
  }

  /// <summary>
  /// Move the value from the current key to the new key
  /// </summary>
  /// <param name="values"></param>
  /// <param name="currentKey"></param>
  /// <param name="newKey"></param>
  /// <returns></returns>
  protected Dictionary<string, object?> MoveValueToNewKey(Dictionary<string, object?> values, string currentKey, string newKey)
  {
    if (!values.TryGetValue(currentKey, out object? value))
    {
      Logger.LogWarning("COuld not find key {currentKey}", currentKey);
      return values;
    }

    values[newKey] = value;
    values.Remove(currentKey);

    return values;
  }
}