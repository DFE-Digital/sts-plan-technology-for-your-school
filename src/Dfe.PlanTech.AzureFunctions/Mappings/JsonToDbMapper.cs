using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public abstract class JsonToDbMapper<TEntity>
where TEntity : ContentComponentDbEntity, new()
{
  protected readonly ILogger<JsonToDbMapper<TEntity>> Logger;
  protected readonly JsonSerializerOptions JsonOptions;

  public JsonToDbMapper(ILogger<JsonToDbMapper<TEntity>> logger, JsonSerializerOptions jsonSerialiserOptions)
  {
    Logger = logger;
    JsonOptions = jsonSerialiserOptions;
  }

  public virtual TEntity MapToEntity(CmsWebHookPayload payload)
  {
    var entity = new TEntity();
    entity = MapSharedFields(payload, entity);

    return MapEntityFields(payload, entity);
  }

  public abstract TEntity MapEntityFields(CmsWebHookPayload payload, TEntity entity);

  public virtual TEntity MapSharedFields(CmsWebHookPayload payload, TEntity entity)
  {
    entity.Id = payload.Sys.Id;

    return entity;
  }

  public IEnumerable<KeyValuePair<string, object>?> GetEntityFields(CmsWebHookPayload payload)
  {
    yield return new KeyValuePair<string, object>("id", payload.Sys.Id);

    foreach (var field in payload.Fields.SelectMany(GetFieldValues))
    {
      yield return field;
    }
  }

  private IEnumerable<KeyValuePair<string, object>?> GetFieldValues(KeyValuePair<string, JsonNode> field)
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

  protected string FirstCharToUpperAsSpan(string input)
  {
    if (string.IsNullOrEmpty(input))
    {
      return string.Empty;
    }
    Span<char> destination = stackalloc char[1];
    input.AsSpan(0, 1).ToUpperInvariant(destination);
    return $"{destination}{input.AsSpan(1)}";
  }

  protected bool TrySerialiseAsLinkEntry(JsonNode node, out CmsWebHookSystemDetailsInner? sys)
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