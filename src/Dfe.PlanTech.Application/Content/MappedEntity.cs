using System.Reflection;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content;

public class MappedEntity(IDatabaseHelper<ICmsDbContext> databaseHelper)
{
    private readonly Type _dontCopyValueAttribute = typeof(DontCopyValueAttribute);

    public required ContentComponentDbEntity IncomingEntity { get; init; }

    public ContentComponentDbEntity? ExistingEntity { get; init; }

    public required CmsEvent CmsEvent { get; init; }

    public bool IsValid { get; private set; }

    public bool AlreadyExistsInDatabase => ExistingEntity != null;

    public bool IsMinimalPayloadEvent => CmsEvent == CmsEvent.UNPUBLISH || CmsEvent == CmsEvent.DELETE;

    public bool ShouldCopyProperties => !IsMinimalPayloadEvent;

    public bool HaveUpdatedProperties { get; private set; }

    /// <summary>
    /// Checks if the incoming entity is a valid component.
    /// </summary>
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
    public void UpdateEntity(IDatabaseHelper<ICmsDbContext> databaseHelper)
    {
        UpdateEntityStatus();
        IsValid = SetDefaultsOnRequiredProperties(databaseHelper, _dontCopyValueAttribute);

        if (ShouldCopyProperties)
        {
            HaveUpdatedProperties = UpdateProperties();
        }
    }

    public (TEntity incoming, TEntity? existing) GetTypedEntities<TEntity>() where TEntity : ContentComponentDbEntity
    {
        if (IncomingEntity is not TEntity incoming)
        {
            throw new InvalidCastException($"Incoming entity is not the expected type. It is {IncomingEntity.GetType()} but expected {typeof(TEntity)}");
        }

        if (ExistingEntity == null)
        {
            return (incoming, null);
        }

        if (ExistingEntity is not TEntity existing)
        {
            throw new InvalidCastException($"Existing entity is not the expected type. It is {IncomingEntity.GetType()} but expected {typeof(TEntity)}");
        }

        return (incoming, existing);
    }


    /// <summary>
    /// Updates the status of the mapped entity based on the provided CMS event, and the existing entity statuses.
    /// </summary>
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
                    throw new InvalidDataException($"Content with Id \"{IncomingEntity.Id}\" has event 'unpublish' despite not existing in the database!");
                }
                IncomingEntity.Published = false;
                break;
            case CmsEvent.DELETE:
                if (ExistingEntity == null)
                {
                    throw new InvalidDataException($"Content with Id \"{IncomingEntity.Id}\" has event 'delete' despite not existing in the database!");
                }
                IncomingEntity.Deleted = true;
                break;
        }
    }

    /// <summary>
    /// Provides a default value for a given type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static object? GetDefaultValue(Type type)
    {
        return type == typeof(string) ? "" : null;
    }

    /// <summary>
    /// Set a default value for any required but null property on the incoming entity
    /// This is to allow saving the entity before it is complete, to avoid losing relationships between entities
    /// </summary>
    /// <param name="db"></param>
    /// <param name="dontCopyAttribute"></param>
    /// <returns>Whether all required properties were successfully updated</returns>
    private bool SetDefaultsOnRequiredProperties(IDatabaseHelper<ICmsDbContext> databaseHelper, Type dontCopyAttribute)
    {
        var existingProperties = databaseHelper.GetRequiredPropertiesForType(IncomingEntity.GetType());

        var missingRequiredProperties = existingProperties.Where(prop => !HasAttribute(dontCopyAttribute, prop)).Where(IncomingPropertyIsNull);

        foreach (var prop in missingRequiredProperties)
        {
            var defaultValue = GetDefaultValue(prop.PropertyType);
            if (defaultValue is null)
                return false;

            prop.SetValue(IncomingEntity, defaultValue);
        }

        return true;
    }

    private bool IncomingPropertyIsNull(PropertyInfo prop) => prop.GetValue(IncomingEntity) == null;

    private static bool HasAttribute(Type attribute, PropertyInfo prop) => prop.CustomAttributes.Any(atr => atr.GetType() == attribute);

    /// <summary>
    /// Updates the properties of the existing entity using the values from the incoming entity.
    /// It only updates the properties if the values have changed, to minimise unnecessary
    /// update calls to the database.
    /// </summary>
    /// <returns> Whether any properties were updated or not </returns>
    private bool UpdateProperties()
    {
        var updatedAnyProperties = false;
        var properties = PropertiesToCopy(IncomingEntity);
        foreach (var (Property, HasDontCopyValueAttribute) in properties)
        {
            if (HasDontCopyValueAttribute)
            {
                databaseHelper.MarkPropertyAsUnchanged(ExistingEntity ?? IncomingEntity, Property.Name);
                continue;
            }

            if (!AlreadyExistsInDatabase)
            {
                continue;
            }

            if (TryUpdateProperty(Property))
            {
                updatedAnyProperties = true;
            }
        }

        return ExistingEntity == null || updatedAnyProperties;
    }

    ///
    ///<returns> Whether the property was updated or not</return>
    private bool TryUpdateProperty(PropertyInfo Property)
    {
        //Get the new and current values from the incoming and existing entities using reflection
        var newValue = Property.GetValue(IncomingEntity);
        var currentValue = Property.GetValue(ExistingEntity);

        // Don't update the existing property if the values are the same
        if (newValue?.Equals(currentValue) == true)
        {
            return false;
        }

        // Update the value of the property in the existing entity with the value from the incoming entity
        Property.SetValue(ExistingEntity, Property.GetValue(IncomingEntity));
        return true;
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
    private IEnumerable<PropertyToModify> PropertiesToCopy(ContentComponentDbEntity entity)
        => entity.GetType()
                .GetProperties()
                .Select(property => new PropertyToModify(property, HasDontCopyValueAttribute(property)));

    /// <summary>
    /// Does the property have a <see cref="DontCopyValueAttribute"/> property attached to it?
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private bool HasDontCopyValueAttribute(PropertyInfo property)
        => property.CustomAttributes.Any(attribute => attribute.AttributeType == _dontCopyValueAttribute);

    private record PropertyToModify(PropertyInfo Property, bool HasDontCopyValueAttribute);
}
