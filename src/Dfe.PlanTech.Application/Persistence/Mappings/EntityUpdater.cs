using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Extensions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class EntityUpdater(ILogger<EntityUpdater> logger, ICmsDbContext db, IDatabaseHelper<ICmsDbContext> databaseHelper)
{
    private readonly ILogger<EntityUpdater> Logger = logger;
    protected readonly ICmsDbContext Db = db;
    protected readonly IDatabaseHelper<ICmsDbContext> DatabaseHelper = databaseHelper;

    /// <summary>
    /// Updates properties on the existing component (if any) based on the incoming component and the CmsEvent
    /// </summary>
    /// <param name="incoming"></param>
    /// <param name="existing"></param>
    /// <param name="cmsEvent"></param>
    /// <returns></returns>
    public async Task<MappedEntity> UpdateEntity(ContentComponentDbEntity incoming, ContentComponentDbEntity? existing, CmsEvent cmsEvent, Func<MappedEntity, Task> postUpdateEntityCallback)
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = incoming,
            ExistingEntity = existing,
            CmsEvent = cmsEvent
        };

        mappedEntity.UpdateEntity(DatabaseHelper);
        await postUpdateEntityCallback(mappedEntity);

        if (!mappedEntity.IsValidComponent(Logger) || mappedEntity.IsMinimalPayloadEvent)
        {
            return mappedEntity;
        }

        mappedEntity = UpdateEntityConcrete(mappedEntity);

        return mappedEntity;
    }

    /// <summary>
    /// Overridable method to allow bespoke updating by entity type if needed
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        return entity;
    }

    public virtual IEnumerable<TReferencedEntity> GetAndOrderReferencedEntities<TReferencedEntity>(Dictionary<string, object?> values, string key)
        where TReferencedEntity : ContentComponentDbEntity, new()
    {
        int order = 0;

        foreach (var referencedEntity in GetReferences<TReferencedEntity>(values, key))
        {
            referencedEntity.Order = order++;
            yield return referencedEntity;
        }
    }

    public virtual IEnumerable<TReferencedEntity> GetReferences<TReferencedEntity>(Dictionary<string, object?> values, string key)
        where TReferencedEntity : ContentComponentDbEntity, new()
    {
        if (!values.TryGetValue(key, out object? referencesArray) || referencesArray is not object[] inners)
        {
            Logger.LogWarning("Expected {Key} to be references array but received {Type}", key, referencesArray?.GetType());
            yield break;
        }

        foreach (var inner in inners)
        {
            if (inner is not string id)
            {
                Logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
                continue;
            }

            yield return new TReferencedEntity() { Id = id };
        }

        values.Remove(key);
    }

    public virtual async Task UpdateReferences<TEntity, TReferencedEntity>(TEntity incomingEntity,
                                                                            TEntity? existingEntity,
                                                                            Func<TEntity, List<TReferencedEntity>> getReferencedEntities,
                                                                            List<TReferencedEntity> incomingReferencedEntities,
                                                                            bool updateOrder = false)
        where TEntity : ContentComponentDbEntity
        where TReferencedEntity : ContentComponentDbEntity
    {
        if (existingEntity != null)
        {
            RemoveRemovedReferencedEntities(existingReferencedEntities: getReferencedEntities(existingEntity), incomingReferencedEntities: incomingReferencedEntities);
        }

        var parentEntity = existingEntity ?? incomingEntity;
        var referenceEntityCollection = getReferencedEntities(existingEntity ?? incomingEntity);

        await AddOrUpdateReferencedEntities(parentEntity, referenceEntityCollection, incomingReferencedEntities, updateOrder);
    }

    protected virtual async Task AddOrUpdateReferencedEntities<TEntity, TReferencedEntity>(TEntity entity,
                                                                                           List<TReferencedEntity> destinationReferenceEntityCollection,
                                                                                           List<TReferencedEntity> incomingReferencedEntities,
                                                                                           bool updateOrder = false)
          where TEntity : ContentComponentDbEntity
          where TReferencedEntity : ContentComponentDbEntity
    {
        int order = 0;

        foreach (var incomingReferencedEntity in incomingReferencedEntities)
        {
            var existingReferencedEntity = destinationReferenceEntityCollection.Find(existingReference => existingReference.Id == incomingReferencedEntity.Id) ?? await AddNewReferencedEntity(incomingReferencedEntity, destinationReferenceEntityCollection);

            if (updateOrder && existingReferencedEntity != null)
            {
                existingReferencedEntity.Order = order;
            }

            order++;
        }

    }

    protected virtual async Task<TReferencedEntity?> AddNewReferencedEntity<TReferencedEntity>(TReferencedEntity referencedEntity, List<TReferencedEntity> referencedEntitiesCollection)
        where TReferencedEntity : ContentComponentDbEntity
    {
        var dbReferencedEntity = await DatabaseHelper.GetMatchingEntityById(referencedEntity, default);
        if (dbReferencedEntity == null)
        {
            Logger.LogError("Error trying to add referenced entity: {ChildReferenceType} {ChildReferenceId} was not found in the db", typeof(TReferencedEntity).Name, referencedEntity.Id);
            return null;
        }

        referencedEntitiesCollection.Add(dbReferencedEntity);
        return dbReferencedEntity;
    }

    protected virtual void RemoveRemovedReferencedEntities<TReferencedEntity>(List<TReferencedEntity> existingReferencedEntities, List<TReferencedEntity> incomingReferencedEntities)
        where TReferencedEntity : ContentComponentDbEntity
    {
        var referencesToRemove = existingReferencedEntities.Where(existingReference => !incomingReferencedEntities.Exists(incomingReference => incomingReference.Id == existingReference.Id))
                                                            .ToArray();

        foreach (var referenceToRemove in referencesToRemove)
        {
            existingReferencedEntities.Remove(referenceToRemove);
        }
    }
}
