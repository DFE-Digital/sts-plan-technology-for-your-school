using System.Linq.Expressions;
using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Extensions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

/// <summary>
/// Maps a JSON payload to the given entity type
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class JsonToDbMapper<TEntity>(
    EntityUpdater entityUpdater,
    ILogger<JsonToDbMapper<TEntity>> logger,
    JsonSerializerOptions jsonSerialiserOptions,
    IDatabaseHelper<ICmsDbContext> databaseHelper)
    : JsonToDbMapper(typeof(TEntity), logger, jsonSerialiserOptions)
    where TEntity : ContentComponentDbEntity, new()
{
    protected CmsWebHookPayload? Payload;
    protected readonly EntityUpdater _entityUpdater = entityUpdater;
    protected IDatabaseHelper<ICmsDbContext> DatabaseHelper => databaseHelper;

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

    public virtual Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override async Task<MappedEntity> MapEntity(CmsWebHookPayload payload, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        var incomingEntity = ToEntity(payload);

        var existingEntity = await GetExistingEntity(incomingEntity, cancellationToken);

        var postUpdatEntityCallback = (MappedEntity mappedEntity) => PostUpdateEntityCallback(mappedEntity, cancellationToken);

        return await _entityUpdater.UpdateEntity(incomingEntity, existingEntity, cmsEvent, postUpdatEntityCallback);
    }

    public virtual Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }

    protected virtual Task<TEntity?> GetExistingEntity(TEntity incomingEntity, CancellationToken cancellationToken)
    => DatabaseHelper.GetMatchingEntityById(incomingEntity, cancellationToken);

    protected virtual Task<List<TDbEntity>> GetEntitiesMatchingPredicate<TDbEntity>(Expression<Func<TDbEntity, bool>> predicate, CancellationToken cancellationToken)
    where TDbEntity : class
    => DatabaseHelper.GetIQueryableForEntityWithoutAutoIncludes<TDbEntity>().Where(predicate).ToListAsync(DatabaseHelper, cancellationToken);

    protected virtual Task<List<TDbEntity>> GetEntitiesMatchingPredicate<TDbEntity, TProperty>(Expression<Func<TDbEntity, bool>> predicate, Expression<Func<TDbEntity, TProperty?>> include, CancellationToken cancellationToken)
        where TDbEntity : class
        where TProperty : class
        => DatabaseHelper.GetIQueryableForEntityWithoutAutoIncludes<TDbEntity>().Where(predicate).Include(include, DatabaseHelper).ToListAsync(DatabaseHelper, cancellationToken);
}
