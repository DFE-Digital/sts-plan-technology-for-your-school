using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class EntityUpdater(ILogger<EntityUpdater> logger, CmsDbContext db)
{
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

        mappedEntity.UpdateEntity(db);
        await postUpdateEntityCallback(mappedEntity);

        if (!mappedEntity.IsValidComponent(logger) || mappedEntity.IsMinimalPayloadEvent)
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
            logger.LogWarning("Expected {Key} to be references array but received {Type}", key, referencesArray?.GetType());
            yield break;
        }

        foreach (var inner in inners)
        {
            if (inner is not string id)
            {
                logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
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
                                                                            DbSet<TReferencedEntity> referencedEntitydbSet,
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

        await AddOrUpdateReferencedEntities(parentEntity, referenceEntityCollection, incomingReferencedEntities, referencedEntitydbSet, updateOrder);
    }

    protected virtual async Task AddOrUpdateReferencedEntities<TEntity, TReferencedEntity>(TEntity entity,
                                                                                           List<TReferencedEntity> destinationReferenceEntityCollection,
                                                                                           List<TReferencedEntity> incomingReferencedEntities,
                                                                                           DbSet<TReferencedEntity> referencedEntitydbSet,
                                                                                           bool updateOrder = false)
          where TEntity : ContentComponentDbEntity
          where TReferencedEntity : ContentComponentDbEntity
    {
        int order = 0;

        foreach (var incomingReferencedEntity in incomingReferencedEntities)
        {
            var existingReferencedEntity = destinationReferenceEntityCollection.Find(existingReference => existingReference.Id == incomingReferencedEntity.Id) ??
                                        await AddNewReferencedEntity(entity, incomingReferencedEntity, destinationReferenceEntityCollection, referencedEntitydbSet);

            if (updateOrder && existingReferencedEntity != null)
            {
                existingReferencedEntity.Order = order;
            }

            order++;
        }

    }

    protected virtual async Task<TReferencedEntity?> AddNewReferencedEntity<TEntity, TReferencedEntity>(TEntity entity, TReferencedEntity referencedEntity, List<TReferencedEntity> referencedEntitiesCollection, DbSet<TReferencedEntity> referencedEntitydbSet)
        where TEntity : ContentComponentDbEntity
        where TReferencedEntity : ContentComponentDbEntity
    {
        var dbReferencedEntity = await referencedEntitydbSet.FirstOrDefaultAsync(dbReferenceEntity => dbReferenceEntity.Id == referencedEntity.Id);
        if (dbReferencedEntity == null)
        {
            logger.LogError("{ParentEntityType} {ParentId} is trying to add {ChildReferenceType} {ChildReferenceId} but this is not found in the db", typeof(TEntity).Name, entity.Id, typeof(TReferencedEntity).Name, referencedEntity.Id);
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
