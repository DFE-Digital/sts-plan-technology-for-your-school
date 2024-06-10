using System.Linq.Expressions;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class EntityUpdater(ILogger<EntityUpdater> logger, CmsDbContext db)
{
    private readonly ILogger<EntityUpdater> _logger = logger;
    protected readonly CmsDbContext Db = db;

    /// <summary>
    /// Updates properties on the existing component (if any) based on the incoming component and the CmsEvent
    /// </summary>
    /// <param name="incoming"></param>
    /// <param name="existing"></param>
    /// <param name="cmsEvent"></param>
    /// <returns></returns>
    public async Task<MappedEntity> UpdateEntity(ContentComponentDbEntity incoming, ContentComponentDbEntity? existing, CmsEvent cmsEvent, Func<MappedEntity, Task> preUpdateEntityCallback)
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = incoming,
            ExistingEntity = existing,
            CmsEvent = cmsEvent
        };

        mappedEntity.UpdateEntity();

        await preUpdateEntityCallback(mappedEntity);

        if (!mappedEntity.IsValidComponent(Db, _logger) || mappedEntity.IsMinimalPayloadEvent)
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
            _logger.LogError("Expected {Key} to be references array but received {Type}", key, referencesArray?.GetType());
            yield break;
        }

        foreach (var inner in inners)
        {
            if (inner is not string id)
            {
                _logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
                continue;
            }

            yield return new TReferencedEntity() { Id = id };
        }

        values.Remove(key);
    }

    public virtual async Task UpdateReferences<TEntity, TReferencedEntity>(TEntity incomingEntity, TEntity? existingEntity, Func<TEntity, List<TReferencedEntity>> getReferencedEntities, List<TReferencedEntity> incomingReferencedEntities, DbSet<TReferencedEntity> referencedEntityDbSet, bool updateOrder = false)
              where TEntity : ContentComponentDbEntity
          where TReferencedEntity : ContentComponentDbEntity
    {
        if (existingEntity == null)
        {
            await AddOrUpdateReferencedEntities(incomingEntity, getReferencedEntities(incomingEntity), incomingReferencedEntities, referencedEntityDbSet, updateOrder);
            return;
        }

        RemoveRemovedReferencedEntities(existingReferencedEntities: getReferencedEntities(existingEntity), incomingReferencedEntities: incomingReferencedEntities);
        await AddOrUpdateReferencedEntities(entity: existingEntity, existingReferencedEntities: getReferencedEntities(existingEntity), incomingReferencedEntities: incomingReferencedEntities, referencedEntityDbSet: referencedEntityDbSet, updateOrder: updateOrder);
    }

    protected virtual async Task AddOrUpdateReferencedEntities<TEntity, TReferencedEntity>(TEntity entity, List<TReferencedEntity> existingReferencedEntities, List<TReferencedEntity> incomingReferencedEntities, DbSet<TReferencedEntity> referencedEntityDbSet, bool updateOrder = false)
          where TEntity : ContentComponentDbEntity
          where TReferencedEntity : ContentComponentDbEntity
    {
        int order = 0;

        foreach (var incomingReferencedEntity in incomingReferencedEntities)
        {
            var existingReferencedEntity = existingReferencedEntities.FirstOrDefault(existingReference => existingReference.Id == incomingReferencedEntity.Id);
            if (existingReferencedEntity == null)
            {
                await AddNewReferencedEntity(entity, existingReferencedEntities, referencedEntityDbSet, incomingReferencedEntity);
                existingReferencedEntity = incomingReferencedEntity;
            }

            if (updateOrder)
            {
                existingReferencedEntity.Order = order;
            }

            order++;
        }

    }

    protected virtual async Task AddNewReferencedEntity<TEntity, TReferencedEntity>(TEntity existingEntity, List<TReferencedEntity> existingReferencedEntities, DbSet<TReferencedEntity> referencedEntityDbSet, TReferencedEntity incomingReferencedEntity)
        where TEntity : ContentComponentDbEntity
        where TReferencedEntity : ContentComponentDbEntity
    {
        var dbReferencedEntity = await referencedEntityDbSet.FirstOrDefaultAsync(referencedEntity => referencedEntity.Id == incomingReferencedEntity.Id);
        if (dbReferencedEntity == null)
        {
            _logger.LogError("{ParentEntityType} {ParentId} is trying to add {ChildReferenceType} {ChildReferenceId} but this is not found in the DB", typeof(TEntity).Name, existingEntity.Id, typeof(TReferencedEntity).Name, incomingReferencedEntity.Id);
            return;
        }

        existingReferencedEntities.Add(dbReferencedEntity);
    }

    protected virtual void RemoveRemovedReferencedEntities<TReferencedEntity>(List<TReferencedEntity> existingReferencedEntities, List<TReferencedEntity> incomingReferencedEntities)
        where TReferencedEntity : ContentComponentDbEntity
    {
        var answersToRemove = existingReferencedEntities.Where(existingReference => !incomingReferencedEntities.Exists(incomingReference => incomingReference.Id == existingReference.Id))
                                                        .ToArray();

        foreach (var answerToRemove in answersToRemove)
        {
            existingReferencedEntities.Remove(answerToRemove);
        }
    }
}