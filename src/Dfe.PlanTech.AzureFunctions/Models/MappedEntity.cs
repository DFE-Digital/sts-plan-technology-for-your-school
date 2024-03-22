using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Dfe.PlanTech.AzureFunctions.Models;

public class MappedEntity
{
    public required ContentComponentDbEntity IncomingEntity { get; init; }

    public ContentComponentDbEntity? ExistingEntity { get; init; }

    public bool IsValid { get; private set; }

    public bool AlreadyExistsInDatabase => ExistingEntity != null;

    /// <summary>
    /// Checks if the incoming entity is a valid component.
    /// </summary>
    /// <param name="db">Database context.</param>
    /// <param name="dontCopyAttribute">The attribute to exclude from the validation.</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>True if the entity is valid, false otherwise.</returns>
    public bool IsValidComponent(CmsDbContext db, Type dontCopyAttribute, ILogger logger)
    {
        // Get a list of null properties from the incoming entity, excluding ones we don't process
        string? nullProperties = string.Join(", ", AnyRequiredPropertyIsNull(db, dontCopyAttribute));

        IsValid = string.IsNullOrEmpty(nullProperties);

        // Log a message if the entity is not valid.
        if (!IsValid)
        {
            logger.LogInformation(
                "Content Component with ID {id} is missing the following required properties: {nullProperties}",
                IncomingEntity.Id,
                nullProperties
            );
        }

        return IsValid;
    }

    /// <summary>
    /// Updates the status of the mapped entity based on the provided CMS event, and the existing entity statuses.
    /// </summary>
    /// <param name="cmsEvent">The event type of the payload.</param>
    public void UpdateEntityStatus(CmsEvent cmsEvent)
    {
        if (ExistingEntity != null)
        {
            CopyEntityStatus();
        }

        UpdateEntityStatusByEvent(cmsEvent);
    }

    /// <summary>
    /// Copies the status values from an existing entity to an incoming entity, to ensure we only
    /// change statuses we need to.
    ///</summary>
    private void CopyEntityStatus()
    {
        IncomingEntity.Archived = ExistingEntity!.Archived;
        IncomingEntity.Published = ExistingEntity!.Published;
        IncomingEntity.Deleted = ExistingEntity!.Deleted;
    }

    /// <summary>
    /// Updates the status of an entity based on the provided CMS event and entity information.
    /// </summary>
    /// <param name="cmsEvent"></param>
    /// <param name="mapped"></param>
    /// <param name="existing"></param>
    /// <exception cref="CmsEventException"></exception>
    private void UpdateEntityStatusByEvent(CmsEvent cmsEvent)
    {

        switch (cmsEvent)
        {
            case CmsEvent.SAVE:
            case CmsEvent.AUTO_SAVE:
                break;
            case CmsEvent.ARCHIVE:
                IncomingEntity.Archived = true;
                break;
            case CmsEvent.UNARCHIVE:
                if (ExistingEntity == null)
                {
                    throw new CmsEventException(string.Format("Content with Id \"{0}\" has event 'unarchive' despite not existing in the database!", IncomingEntity.Id));
                }
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

    private IEnumerable<PropertyInfo?> AnyRequiredPropertyIsNull(CmsDbContext db, Type dontCopyAttribute)
        => db.Model.FindEntityType(IncomingEntity.GetType())!
                    .GetProperties()
                    .Where(prop => !prop.IsNullable)
                    .Select(prop => prop.PropertyInfo)
                    .Where(prop => !prop!.CustomAttributes.Any(atr => atr.GetType() == dontCopyAttribute))
                    .Where(prop => prop!.GetValue(IncomingEntity) == null);
}