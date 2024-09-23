using System.Reflection;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Models;

public class MappedEntity
{
    private readonly Type _dontCopyValueAttribute = typeof(DontCopyValueAttribute);

    public required ContentComponentDbEntity IncomingEntity { get; init; }

    public ContentComponentDbEntity? ExistingEntity { get; init; }

    public required CmsEvent CmsEvent { get; init; }

    public bool IsValid { get; private set; }

    public bool AlreadyExistsInDatabase => ExistingEntity != null;

    public bool IsMinimalPayloadEvent => CmsEvent == CmsEvent.UNPUBLISH || CmsEvent == CmsEvent.DELETE;

    public bool ShouldCopyProperties => AlreadyExistsInDatabase && !IsMinimalPayloadEvent;

    /// <summary>
    /// Checks if the incoming entity is a valid component.
    /// </summary>
    /// <param name="db">Database context.</param>
    /// <param name="dontCopyAttribute">The attribute to exclude from the validation.</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>True if the entity is valid, false otherwise.</returns>
    public bool IsValidComponent(ILogger logger)
    {
        if (IsMinimalPayloadEvent)
        {
            IsValid = true;
            return true;
        }

        if (!IsValid)
            logger.LogWarning("Content Component with ID {Id} has required properties that could not be set to a default value", IncomingEntity.Id);

        return IsValid;
    }

    /// <summary>
    /// Sets defaults on the incoming entity before properties are copied to the existing one
    /// </summary>
    public void UpdateEntity(CmsDbContext db)
    {
        UpdateEntityStatus();
        IsValid = SetDefaultsOnRequiredProperties(db, _dontCopyValueAttribute);

        if (ShouldCopyProperties)
        {
            UpdateProperties();
        }
    }

    public (TEntity incoming, TEntity? existing) GetTypedEntities<TEntity>()
        where TEntity : ContentComponentDbEntity
    {
        if (IncomingEntity is not TEntity incoming)
        {
            throw new InvalidCastException($"Entities are not expected type. Received {IncomingEntity.GetType()} and {ExistingEntity!.GetType()} but expected {typeof(TEntity)}");
        }

        if (ExistingEntity == null)
        {
            return (incoming, null);
        }

        if (ExistingEntity is not TEntity existing)
        {
            throw new InvalidCastException($"Entities are not expected type. Received  {IncomingEntity.GetType()} and  {ExistingEntity!.GetType()} but expected  {typeof(TEntity)}");
        }

        return (incoming, existing);
    }


    /// <summary>
    /// Updates the status of the mapped entity based on the provided CMS event, and the existing entity statuses.
    /// </summary>
    /// <param name="cmsEvent">The event type of the payload.</param>
    private void UpdateEntityStatus()
    {
        if (ExistingEntity != null)
        {
            CopyEntityStatus(ExistingEntity, IncomingEntity);
        }

        UpdateEntityStatusByEvent();
    }

    /// <summary>
    /// Copies the status values one entity to another, to ensure that they match when updated.
    ///</summary>
    private static void CopyEntityStatus(ContentComponentDbEntity source, ContentComponentDbEntity target)
    {
        target.Archived = source.Archived;
        target.Published = source.Published;
        target.Deleted = source.Deleted;
    }

    /// <summary>
    /// Updates the status of an entity based on the provided CMS event and entity information.
    /// </summary>
    /// <param name="cmsEvent"></param>
    /// <param name="mapped"></param>
    /// <param name="existing"></param>
    /// <exception cref="CmsEventException"></exception>
    private void UpdateEntityStatusByEvent()
    {
        switch (CmsEvent)
        {
            case CmsEvent.SAVE:
            case CmsEvent.AUTO_SAVE:
                break;
            case CmsEvent.ARCHIVE:
                IncomingEntity.Archived = true;
                break;
            case CmsEvent.UNARCHIVE:
                IncomingEntity.Archived = false;
                break;
            case CmsEvent.PUBLISH:
                IncomingEntity.Published = true;
                break;
            case CmsEvent.UNPUBLISH:
                if (ExistingEntity == null)
                {
                    throw new CmsEventException(string.Format("Content with Id \"{0}\" has event 'unpublish' despite not existing in the database!", IncomingEntity.Id));
                }
                IncomingEntity.Published = false;
                break;
            case CmsEvent.DELETE:
                if (ExistingEntity == null)
                {
                    throw new CmsEventException(string.Format("Content with Id \"{0}\" has event 'delete' despite not existing in the database!", IncomingEntity.Id));
                }
                IncomingEntity.Deleted = true;
                break;
        }
    }

    /// <summary>
    /// Provides a default value for a given type.
    /// Ints and Bools default to 0 and false without intervention
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static object? GetDefaultValue(Type type)
    {
        if (type == typeof(string))
            return "";

        return null;
    }

    /// <summary>
    /// Set a default value for any required but null property on the incoming entity
    /// This is to allow saving the entity before it is complete, to avoid losing relationships between entities
    /// </summary>
    /// <param name="db"></param>
    /// <param name="dontCopyAttribute"></param>
    /// <returns>Whether all required properties were successfully updated</returns>
    private bool SetDefaultsOnRequiredProperties(CmsDbContext db, Type dontCopyAttribute)
    {
        var existingProperties = new List<PropertyInfo>();
        var missingRequiredProperties = existingProperties.Where(prop => prop != null && prop.CustomAttributes.All(atr => atr.GetType() != dontCopyAttribute)).Where(prop => prop!.GetValue(IncomingEntity) == null);

        foreach (var prop in missingRequiredProperties)
        {
            var defaultValue = GetDefaultValue(prop!.PropertyType);
            if (defaultValue is null)
                return false;
            prop.SetValue(IncomingEntity, defaultValue);
        }

        return true;
    }

    /// <summary>
    /// Updates the properties of the existing entity using the values from the incoming entity.
    /// It only updates the properties if the values have changed, to minimise unnecessary
    /// update calls to the database.
    /// </summary>
    private void UpdateProperties()
    {
        foreach (var property in PropertiesToCopy(IncomingEntity))
        {
            //Get the new and current values from the incoming and existing entities using reflection
            var newValue = property.GetValue(IncomingEntity);
            var currentValue = property.GetValue(ExistingEntity);

            // Don't update the existing property if the values are the same
            if (newValue?.Equals(currentValue) == true)
            {
                continue;
            }

            // Update the value of the property in the existing entity with the value from the incoming entity
            property.SetValue(ExistingEntity, property.GetValue(IncomingEntity));
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
