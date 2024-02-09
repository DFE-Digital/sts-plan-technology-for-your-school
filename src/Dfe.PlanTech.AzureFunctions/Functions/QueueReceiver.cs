using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;

namespace Dfe.PlanTech.AzureFunctions;

public class QueueReceiver : BaseFunction
{
    private readonly CmsDbContext _db;
    private readonly ContentfulOptions _contentfulOptions = new(true);
    private readonly JsonToEntityMappers _mappers;
    private readonly Type _dontCopyValueAttribute = typeof(DontCopyValueAttribute);

    public QueueReceiver(ContentfulOptions contentfulOptions, ILoggerFactory loggerFactory, CmsDbContext db, JsonToEntityMappers mappers) : base(loggerFactory.CreateLogger<QueueReceiver>())
    {
        _contentfulOptions = contentfulOptions;
        _db = db;
        _mappers = mappers;
    }

    /// <summary>
    /// Azure Function App function that processes messages from a Service Bus queue, converts them
    /// to the appropriate <see cref="ContentComponentDbEntity"/> class, and adds/updates the database where appropriate.
    /// </summary>
    /// <param name="messages">Array of ServiceBusReceivedMessage objects representing the messages to be processed</param>
    /// <param name="messageActions">object providing actions for handling the messages.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("QueueReceiver")]
    public async Task QueueReceiverDbWriter([ServiceBusTrigger("contentful", IsBatched = true)] ServiceBusReceivedMessage[] messages, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Queue Receiver -> Db Writer started. Processing {msgCount} messages", messages.Length);

        foreach (ServiceBusReceivedMessage message in messages)
        {
            await ProcessMessage(message, messageActions, cancellationToken);
        }
    }

    /// <summary>
    /// Processes a message from the Service Bus, updating the corresponding database entities,
    /// and completing or dead-lettering the message. 
    /// </summary>
    /// <param name="message">Service bus message to process</param>
    /// <param name="messageActions">Service bus message actions for completing or dead-lettering</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ProcessMessage(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        try
        {
            CmsEvent cmsEvent = GetCmsEvent(message.Subject);

            if (ShouldIgnoreMessage(cmsEvent))
            {
                await messageActions.CompleteMessageAsync(message, cancellationToken);
                return;
            }

            ContentComponentDbEntity mapped = MapMessageToEntity(message);
            ContentComponentDbEntity? existing = await TryGetExistingEntity(mapped, cancellationToken);

            if (!IsNewAndValidComponent(mapped, existing))
            {
                await messageActions.CompleteMessageAsync(message, cancellationToken);
                return;
            }

            UpdateEntityStatusByEvent(cmsEvent, mapped, existing);

            UpsertEntity(cmsEvent, mapped, existing);

            await DbSaveChanges(cancellationToken);

            await messageActions.CompleteMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
            await messageActions.DeadLetterMessageAsync(message, null, ex.Message, ex.StackTrace, cancellationToken);
        }
    }

    private bool IsNewAndValidComponent(ContentComponentDbEntity mapped, ContentComponentDbEntity? existing)
    {
        if (existing != null) return true;

        if (AnyRequiredPropertyIsNull(mapped, out List<PropertyInfo?> nullProperties))
        {
            Logger.LogInformation("Content Component with ID {id} is missing the following required properties: {nullProperties}", mapped.Id, string.Join(", ", nullProperties));
            return false;
        }

        return true;
    }

    private bool AnyRequiredPropertyIsNull(ContentComponentDbEntity entity, out List<PropertyInfo?> nullProperties)
    {
        nullProperties = _db.Model.FindEntityType(entity.GetType())!
                        .GetProperties()
                        .Where(prop => !prop.IsNullable)
                        .Select(prop => prop.PropertyInfo)
                        .Where(prop => !prop!.CustomAttributes.Any(atr => atr.GetType() == typeof(DontCopyValueAttribute)))
                        .Where(prop => prop!.GetValue(entity) == null)
                        .ToList();

        return nullProperties.Count > 0;
    }

    // private bool AnyRequiredPropertyIsNull(ContentComponentDbEntity entity)
    // => _db.Model.FindEntityType(entity.GetType())!
    //     .GetProperties()
    //     .Where(prop => !prop.IsNullable)
    //     .Select(prop => prop.PropertyInfo)
    //     .Where(prop => !prop!.CustomAttributes.Any(atr => atr.GetType() == typeof(DontCopyValueAttribute)))
    //     .Where(prop => prop!.GetValue(entity) == null)
    //     .Any();

    /// <summary>
    /// Checks if the message with the given CmsEvent should be ignored based on certain conditions.
    /// </summary>
    /// <remarks>
    /// If we recieve a create event, we return true.
    /// If we are NOT using preview mode (i.e. we are ignoring drafts), and the event is just a save or autosave, then return true.
    /// 
    /// Else, we return false.
    /// </remarks>
    /// <param name="cmsEvent"></param>
    /// <returns></returns>
    private bool ShouldIgnoreMessage(CmsEvent cmsEvent)
    {
        if (cmsEvent == CmsEvent.CREATE)
        {
            Logger.LogInformation("Dropping received event {cmsEvent}", cmsEvent);
            return true;
        }

        if ((cmsEvent == CmsEvent.SAVE || cmsEvent == CmsEvent.AUTO_SAVE) && !_contentfulOptions.UsePreview)
        {
            Logger.LogInformation("Receieved {event} but UsePreview is {usePreview} - dropping message", cmsEvent, _contentfulOptions.UsePreview);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Retrieves the CmsEvent based on the provided subject. 
    /// </summary>
    /// <param name="contentfulEvent">The Contentful event (from the service bus message subject)</param>
    /// <returns></returns>
    /// <exception cref="CmsEventException">Exception thrown if we are unable to parse the event to a valid <see cref="CmsEvent"/></exception>
    private CmsEvent GetCmsEvent(string contentfulEvent)
    {
        if (!Enum.TryParse(contentfulEvent.AsSpan()[(contentfulEvent.LastIndexOf('.') + 1)..], true, out CmsEvent cmsEvent))
        {
            throw new CmsEventException(string.Format("Cannot parse header \"{0}\" into a valid CMS event", contentfulEvent));
        }

        Logger.LogInformation("CMS Event: {cmsEvent}", cmsEvent);

        return cmsEvent;
    }

    /// <summary>
    /// Maps the incoming message to the appropriate <see cref="ContentComponentDbEntity"/>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>

    private ContentComponentDbEntity MapMessageToEntity(ServiceBusReceivedMessage message)
    {
        string messageBody = Encoding.UTF8.GetString(message.Body);

        Logger.LogInformation("Processing = {messageBody}", messageBody);

        return _mappers.ToEntity(messageBody);
    }

    /// <summary>
    /// Asynchronously tries to retrieve an existing ContentComponentDbEntity instance
    /// </summary>
    /// <param name="mapped"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<ContentComponentDbEntity?> TryGetExistingEntity(ContentComponentDbEntity mapped, CancellationToken cancellationToken)
    {
        ContentComponentDbEntity? existing = await GetExistingDbEntity(mapped, cancellationToken);

        if (existing != null)
        {
            mapped.Archived = existing.Archived;
            mapped.Published = existing.Published;
            mapped.Deleted = existing.Deleted;
        }

        return existing;
    }

    /// <summary>
    /// Gets the existing entity (if existing) from the database that matches the mapped entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Exception thrown when we do not have a matching DbSet in our DbContext for the entity type</exception>
    private async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
    {
        var model = _db.Model.FindEntityType(entity.GetType()) ?? throw new KeyNotFoundException($"Could not find model in database for {entity.GetType()}");

        var dbSet = GetIQueryableForEntity(model);

        var found = await dbSet.IgnoreAutoIncludes()
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(existing => existing.Id == entity.Id, cancellationToken);

        return found ?? null;
    }

    /// <summary>
    /// Uses reflection to get the DbSet, as an IQueryable, for the provided entity 
    /// </summary>
    /// <param name="model">Entity type for the entity we have received</param>
    /// <returns></returns>
    private IQueryable<ContentComponentDbEntity> GetIQueryableForEntity(IEntityType model)
    => (IQueryable<ContentComponentDbEntity>)_db
                                    .GetType()
                                    .GetMethod("Set", 1, Type.EmptyTypes)!
                                    .MakeGenericMethod(model!.ClrType)!
                                    .Invoke(_db, null)!;

    /// <summary>
    /// Updates the status of an entity based on the provided CMS event and entity information.
    /// </summary>
    /// <param name="cmsEvent"></param>
    /// <param name="mapped"></param>
    /// <param name="existing"></param>
    /// <exception cref="CmsEventException"></exception>
    private static void UpdateEntityStatusByEvent(CmsEvent cmsEvent, ContentComponentDbEntity mapped, ContentComponentDbEntity? existing)
    {
        switch (cmsEvent)
        {
            case CmsEvent.SAVE:
            case CmsEvent.AUTO_SAVE:
                break;
            case CmsEvent.ARCHIVE:
                mapped.Archived = true;
                break;
            case CmsEvent.UNARCHIVE:
                mapped.Archived = false;
                break;
            case CmsEvent.PUBLISH:
                mapped.Published = true;
                break;
            case CmsEvent.UNPUBLISH:
                if (existing == null)
                {
                    throw new CmsEventException(string.Format("Content with Id \"{0}\" has event 'unpublish' despite not existing in the database!", mapped.Id));
                }
                existing.Published = false;
                break;
            case CmsEvent.DELETE:
                mapped.Deleted = true;
                break;
        }
    }

    /// <summary>
    /// Either add the entity to DB, or update the properties of the existing entity
    /// </summary>
    /// <param name="cmsEvent"></param>
    /// <param name="mapped"></param>
    /// <param name="existing"></param>
    private void UpsertEntity(CmsEvent cmsEvent, ContentComponentDbEntity mapped, ContentComponentDbEntity? existing)
    {
        if (cmsEvent == CmsEvent.UNPUBLISH) return;

        if (existing == null)
        {
            DbAdd(mapped);
        }
        else
        {
            UpdateProperties(mapped, existing);
        }
    }

    private void DbAdd(ContentComponentDbEntity mapped)
    {
        _db.Add(mapped);
    }

    private void UpdateProperties(ContentComponentDbEntity incoming, ContentComponentDbEntity existing)
    {
        foreach (var property in PropertiesToCopy(incoming))
        {
            var newValue = property.GetValue(incoming);
            var currentValue = property.GetValue(existing);
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
    /// Saves changes in database, and logs information about rows changed
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task DbSaveChanges(CancellationToken cancellationToken)
    {
        long rowsChangedInDatabase = await _db.SaveChangesAsync(cancellationToken);

        if (rowsChangedInDatabase == 0L)
        {
            Logger.LogWarning("No rows changed in the database!");
        }
        else
        {
            Logger.LogInformation($"Updated {rowsChangedInDatabase} rows in the database");
        }
    }
}