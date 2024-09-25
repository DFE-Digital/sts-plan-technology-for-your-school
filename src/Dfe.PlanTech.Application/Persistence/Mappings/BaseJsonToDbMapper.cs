using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public abstract class JsonToDbMapper(Type entityType, ILogger logger, JsonSerializerOptions jsonSerialiserOptions)
{
    protected readonly ILogger Logger = logger;
    protected readonly JsonSerializerOptions JsonOptions = jsonSerialiserOptions;
    private readonly Type _entityType = entityType;

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

        if (payload.Sys.Type.Equals("DeletedEntry"))
            yield break;

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
        if (field.Value is null)
        {
            Logger.LogError("No value for {field}", field.Key);
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
            yield break;
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
            Logger.LogError(ex, "Error when serialising field \"{FieldName}\":as object.\nValue was: {FieldValue}", field.Key, field.Value);
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
            Logger.LogWarning("Could not find key {currentKey}", currentKey);
            return values;
        }

        values[newKey] = value;
        values.Remove(currentKey);

        return values;
    }
}
