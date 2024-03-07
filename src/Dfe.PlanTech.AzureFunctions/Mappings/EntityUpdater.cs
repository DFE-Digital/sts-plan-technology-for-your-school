using System.Reflection;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
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
  public MappedEntity UpdateEntity(ContentComponentDbEntity incoming, ContentComponentDbEntity? existing, CmsEvent cmsEvent)
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

  private void CopyEntityStatus()
  {
    _mappedEntity!.IncomingEntity.Archived = _mappedEntity.ExistingEntity!.Archived;
    _mappedEntity!.IncomingEntity.Published = _mappedEntity.ExistingEntity!.Published;
    _mappedEntity!.IncomingEntity.Deleted = _mappedEntity.ExistingEntity!.Deleted;
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
}