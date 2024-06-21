using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

/// <summary>
/// Maps a JSON payload to the given entity type
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class JsonToDbMapper<TEntity>(EntityRetriever entityRetriever, EntityUpdater entityUpdater, ILogger<JsonToDbMapper<TEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper(entityRetriever, typeof(TEntity), logger, jsonSerialiserOptions)
where TEntity : ContentComponentDbEntity, new()
{
    protected CmsWebHookPayload? Payload;
    protected EntityUpdater _entityUpdater = entityUpdater;

    public virtual TEntity ToEntity(CmsWebHookPayload payload)
    {
        Payload = payload;

        var values = GetEntityValuesDictionary(payload);

        if (!payload.Sys.Type.Equals("DeletedEntry"))
        {
            values = PerformAdditionalMapping(values);
        }

        var asJson = JsonSerializer.Serialize(values, JsonOptions);
        var serialised = JsonSerializer.Deserialize<TEntity>(asJson, JsonOptions) ?? throw new ArgumentNullException("Null returned");

        return serialised;
    }

    public virtual Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        return Task.CompletedTask;
    }

    public override async Task<MappedEntity> MapEntity(CmsWebHookPayload payload, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        var incomingEntity = ToEntity(payload);

        var existingEntity = await EntityRetriever.GetExistingDbEntity(incomingEntity, cancellationToken);

        return await _entityUpdater.UpdateEntity(incomingEntity, existingEntity, cmsEvent, PostUpdateEntityCallback);
    }

    public virtual Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}

public abstract class JsonToDbMapper
{
    protected readonly EntityRetriever EntityRetriever;
    protected readonly ILogger Logger;
    protected readonly JsonSerializerOptions JsonOptions;
    private readonly Type _entityType;

    protected JsonToDbMapper(EntityRetriever entityRetriever, Type entityType, ILogger logger, JsonSerializerOptions jsonSerialiserOptions)
    {
        EntityRetriever = entityRetriever;
        _entityType = entityType;
        Logger = logger;
        JsonOptions = jsonSerialiserOptions;
    }

    public virtual bool AcceptsContentType(string contentType)
      => _entityType.Name.Equals($"{contentType}DbEntity", StringComparison.InvariantCultureIgnoreCase);

    public abstract Task<MappedEntity> MapEntity(CmsWebHookPayload payload, CmsEvent cmsEvent, CancellationToken cancellationToken);

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

        if (payload.Sys.Type.Equals("DeletedEntry")) yield break;

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
            yield break;
        }

        var fieldChildren = GetValueAsObject(field);

        if (fieldChildren == null)
        {
            yield return null;
            yield break;
        }

        if (fieldChildren!.Count > 1)
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

    private JsonObject? GetValueAsObject(KeyValuePair<string, JsonNode> field)
    {
        try
        {
            return field.Value!.AsObject();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Errorwhen serialising field \"{FieldName}\":as object.\nValue was: {FieldValue}", field.Key, field.Value);
            return null;
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

        //If it's an entity reference, just us the Id field
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

        return !string.IsNullOrEmpty(sys.Id);
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