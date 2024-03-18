using System.Linq.Expressions;
using System.Reflection;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class EntityUpdater(ILogger<EntityUpdater> logger, CmsDbContext db)
{
  private readonly Type _dontCopyValueAttribute = typeof(DontCopyValueAttribute);
  private readonly ILogger<EntityUpdater> _logger = logger;
  private MappedEntity? _mappedEntity;

  protected readonly CmsDbContext Db = db;

  /// <summary>
  /// Updates properties on the existing component (if any) based on the incoming component and the CmsEvent
  /// </summary>
  /// <param name="incoming"></param>
  /// <param name="existing"></param>
  /// <param name="cmsEvent"></param>
  /// <returns></returns>
  public async Task<MappedEntity> UpdateEntity(ContentComponentDbEntity incoming, ContentComponentDbEntity? existing, CmsEvent cmsEvent)
  {
    var mappedEntity = new MappedEntity()
    {
      IncomingEntity = incoming,
      ExistingEntity = existing
    };

    _mappedEntity = mappedEntity;

    if (mappedEntity.AlreadyExistsInDatabase)
    {
      CopyEntityStatus();
      UpdateProperties(incoming, existing!);
    }

    UpdateEntityStatusByEvent(cmsEvent);

    mappedEntity.IsValidComponent(Db, _dontCopyValueAttribute, _logger);

    mappedEntity = await UpdateEntityConcrete(mappedEntity);

    return mappedEntity;
  }

  /// <summary>
  /// Overridable method to allow bespoke updating by entity type if needed
  /// </summary>
  /// <param name="entity"></param>
  /// <returns></returns>
  public virtual Task<MappedEntity> UpdateEntityConcrete(MappedEntity entity)
  {
    return Task.FromResult(entity);
  }

  /// <summary>
  /// Updates the status of an entity based on the provided CMS event and entity information.
  /// </summary>
  /// <param name="cmsEvent"></param>
  /// <param name="mapped"></param>
  /// <param name="existing"></param>
  /// <exception cref="CmsEventException"></exception>
  public void UpdateEntityStatusByEvent(CmsEvent cmsEvent)
  {
    if (_mappedEntity == null) throw new InvalidDataException($"{nameof(_mappedEntity)} is null");

    switch (cmsEvent)
    {
      case CmsEvent.SAVE:
      case CmsEvent.AUTO_SAVE:
        break;
      case CmsEvent.ARCHIVE:
        _mappedEntity.IncomingEntity.Archived = true;
        break;
      case CmsEvent.UNARCHIVE:
        if (_mappedEntity.ExistingEntity == null)
        {
          throw new CmsEventException(string.Format("Content with Id \"{0}\" has event 'unarchive' despite not existing in the database!", _mappedEntity.IncomingEntity.Id));
        }

        _mappedEntity.IncomingEntity.Archived = false;
        break;
      case CmsEvent.PUBLISH:
        _mappedEntity.IncomingEntity.Published = true;
        break;
      case CmsEvent.UNPUBLISH:
        if (_mappedEntity.ExistingEntity == null)
        {
          throw new CmsEventException(string.Format("Content with Id \"{0}\" has event 'unpublish' despite not existing in the database!", _mappedEntity.IncomingEntity.Id));
        }
        _mappedEntity.ExistingEntity.Published = false;
        break;
      case CmsEvent.DELETE:
        if (_mappedEntity.ExistingEntity == null)
        {
          throw new CmsEventException(string.Format("Content with Id \"{0}\" has event 'delete' despite not existing in the database!", _mappedEntity.IncomingEntity.Id));
        }
        _mappedEntity.ExistingEntity.Deleted = true;
        break;
    }
  }

  protected void RemoveOldRelationships<TIncomingEntity, TRelationshipTableEntity>(TIncomingEntity incoming, Func<CmsDbContext, DbSet<TRelationshipTableEntity>> getRelationshipsTable, Func<TRelationshipTableEntity, TIncomingEntity, bool> relationshipExists)
    where TRelationshipTableEntity : class, IDbEntity
  {
    var relationalDbSet = getRelationshipsTable(Db);

    var relationsToRemove = relationalDbSet.Where(relation => !relationshipExists(relation, incoming));

    relationalDbSet.RemoveRange(relationsToRemove);
  }

  protected async Task AddNewRelationshipsAndRemoveDuplicates<TIncomingEntity, TRelationshipEntity, TRelationshipTableEntity>(TIncomingEntity incoming,
                                                                                                                              TIncomingEntity existing,
                                                                                                                              Func<CmsDbContext, DbSet<TRelationshipTableEntity>> getRelationshipsTable,
                                                                                                                              Func<TIncomingEntity, List<TRelationshipEntity>> selectRelationships,
                                                                                                                              Func<DbSet<TRelationshipTableEntity>, TIncomingEntity, TRelationshipEntity, IQueryable<TRelationshipTableEntity>> getExistingRelationships,
                                                                                                                              Action<TRelationshipEntity, TRelationshipTableEntity>? callbackForMatchedRelationship = null)
    where TRelationshipTableEntity : class, IDbEntity
    where TRelationshipEntity : class, new()
  {
    var dbSet = getRelationshipsTable(Db);

    foreach (var relationship in selectRelationships(incoming))
    {
      var matching = await getExistingRelationships(dbSet, incoming, relationship).OrderBy(relation => relation.Id).ToArrayAsync();

      if (matching.Length == 0)
      {
        selectRelationships(existing).Add(relationship);
        continue;
      }

      if (matching.Length > 1)
      {
        dbSet.RemoveRange(matching[1..]);
      }

      callbackForMatchedRelationship?.Invoke(relationship, matching[0]);
    }
  }

  protected static (TEntity incoming, TEntity existing) MapToConcreteType<TEntity>(MappedEntity mapped)
  {
    static string generateErrorMessage(ContentComponentDbEntity entity) => $"Entity is not the expected type. Expected {typeof(TEntity)} but is actually {entity!.GetType()}";

    if (mapped.ExistingEntity is not TEntity existing)
    {
      throw new InvalidCastException(generateErrorMessage(mapped.ExistingEntity!));
    }

    if (mapped.IncomingEntity is not TEntity incoming)
    {
      throw new InvalidCastException(generateErrorMessage(mapped.ExistingEntity!));
    }

    return (incoming, existing);
  }

  private void UpdateProperties(ContentComponentDbEntity incoming, ContentComponentDbEntity existing)
  {
    foreach (var property in PropertiesToCopy(incoming))
    {
      var newValue = property.GetValue(incoming);
      var currentValue = property.GetValue(existing);

      //Don't update existing properties if they haven't changed,
      //to minimise unnecessary update calls to DB
      if (newValue?.Equals(currentValue) == true)
      {
        continue;
      }
      property.SetValue(existing, property.GetValue(incoming));
    }
  }

  /// <summary>
  /// Get properties to copy for the selected entity
  /// </summary>
  /// <remarks>
  /// Returns all properties, except properties ending with "Id" (i.e. relationship fields), and properties that have
  /// a <see cref="DontCopyValueAttribute"/> attribute.
  /// </remarks>
  /// <param name="entity">Entity to get copyable properties for</param>
  /// <returns></returns>
  private IEnumerable<PropertyInfo> PropertiesToCopy(ContentComponentDbEntity entity)
  => entity.GetType()
          .GetProperties()
          .Where(property => !HasDontCopyValueAttribute(property));

  /// <summary>
  /// Does the property have a <see cref="DontCopyValueAttribute"/> property attached to it? 
  /// </summary>
  /// <param name="property"></param>
  /// <returns></returns>
  private bool HasDontCopyValueAttribute(PropertyInfo property)
   => property.CustomAttributes.Any(attribute => attribute.AttributeType == _dontCopyValueAttribute);

  /// <summary>
  /// Copies the status of the entity from the existing entity to the incoming entity.
  /// </summary>
  private void CopyEntityStatus()
  {
    _mappedEntity!.IncomingEntity.Archived = _mappedEntity.ExistingEntity!.Archived;
    _mappedEntity!.IncomingEntity.Published = _mappedEntity.ExistingEntity!.Published;
    _mappedEntity!.IncomingEntity.Deleted = _mappedEntity.ExistingEntity!.Deleted;
  }
}