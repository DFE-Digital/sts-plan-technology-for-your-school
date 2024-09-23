using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

/// <summary>
/// Maps a JSON payload to the given entity type
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class JsonToDbMapper<TEntity>(
    EntityRetriever entityRetriever,
    EntityUpdater entityUpdater,
    ILogger<JsonToDbMapper<TEntity>> logger,
    JsonSerializerOptions jsonSerialiserOptions)
    : BaseJsonToDbMapper(entityRetriever, typeof(TEntity), logger, jsonSerialiserOptions)
    where TEntity : ContentComponentDbEntity, new()
{
    protected CmsWebHookPayload? Payload;
    protected readonly EntityUpdater _entityUpdater = entityUpdater;

    public virtual TEntity ToEntity(CmsWebHookPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        Payload = payload;

        var values = GetEntityValuesDictionary(payload);

        if (!payload.Sys.Type.Equals("DeletedEntry"))
        {
            values = PerformAdditionalMapping(values);
        }

        var asJson = JsonSerializer.Serialize(values, JsonOptions);
        var serialised = JsonSerializer.Deserialize<TEntity>(asJson, JsonOptions) ?? throw new JsonException("Deserialization returned null");

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
