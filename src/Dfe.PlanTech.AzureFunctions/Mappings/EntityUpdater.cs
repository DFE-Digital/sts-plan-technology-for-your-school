using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class EntityUpdater(ILogger<EntityUpdater> logger, CmsDbContext db)
{
    private readonly Type _dontCopyValueAttribute = typeof(DontCopyValueAttribute);
    private readonly ILogger<EntityUpdater> _logger = logger;
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

        mappedEntity.UpdateEntityStatus(cmsEvent);

        if (mappedEntity.AlreadyExistsInDatabase)
        {
            UpdateProperties(incoming, existing!);
        }

        if (!mappedEntity.IsValidComponent(Db, _dontCopyValueAttribute, _logger))
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

    /// <summary>
    /// Updates the properties of the existing entity using the values from the incoming entity.
    /// It only updates the properties if the values have changed, to minimise unnecessary 
    /// update calls to the database.
    /// </summary>
    /// <param name="incoming">The incoming entity with the new values.</param>
    /// <param name="existing">The existing entity with the current values.</param>
    private void UpdateProperties(ContentComponentDbEntity incoming, ContentComponentDbEntity existing)
    {
        foreach (var property in PropertiesToCopy(incoming))
        {
            //Get the new and current values from the incoming and existing entities using reflection
            var newValue = property.GetValue(incoming);
            var currentValue = property.GetValue(existing);

            // Don't update the existing property if the values are the same
            if (newValue?.Equals(currentValue) == true)
            {
                continue;
            }

            // Update the value of the property in the existing entity with the value from the incoming entity
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