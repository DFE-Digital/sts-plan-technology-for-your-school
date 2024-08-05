using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.AzureFunctions.Services;
using Dfe.PlanTech.AzureFunctions.Utils;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.AzureFunctions;

public class QueueReceiver(
    ContentfulOptions contentfulOptions,
    ILoggerFactory loggerFactory,
    CmsDbContext db,
    JsonToEntityMappers mappers,
    IMessageRetryHandler messageRetryHandler,
    ICacheHandler cacheHandler)
    : BaseFunction(loggerFactory.CreateLogger<QueueReceiver>())
{
    public QueueReceiver(IOptions<ContentfulOptions> contentfulOptions,
    ILoggerFactory loggerFactory,
    CmsDbContext db,
    JsonToEntityMappers mappers,
    IMessageRetryHandler messageRetryHandler,
    ICacheHandler cacheHandler) : this(contentfulOptions.Value, loggerFactory, db, mappers, messageRetryHandler, cacheHandler)
    {

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
    public async Task QueueReceiverDbWriter(
        [ServiceBusTrigger("contentful", IsBatched = true)] ServiceBusReceivedMessage[] messages,
        ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Queue Receiver -> Db Writer started. Processing {MsgCount} messages", messages.Length);

        foreach (ServiceBusReceivedMessage message in messages)
        {
            await ProcessMessage(message, messageActions, cancellationToken);
        }
    }

    /// <summary>
    /// Processes a message from the Service Bus, updating the corresponding database entities,
    /// and completing or dead-lettering the message.
    /// Then makes a request to the web app to clear the database cache.
    /// </summary>
    /// <param name="message">Service bus message to process</param>
    /// <param name="messageActions">Service bus message actions for completing or dead-lettering</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ProcessMessage(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        try
        {
            CmsEvent cmsEvent = GetCmsEvent(message.Subject);
            MappedEntity mapped = await MapMessageToEntity(message, cmsEvent, cancellationToken);

            var isPublished = mapped.ExistingEntity?.Published ?? false;

            if (ShouldIgnoreMessage(cmsEvent, isPublished))
            {
                await messageActions.CompleteMessageAsync(message, cancellationToken);
                return;
            }

            if (mapped.IsMinimalPayloadEvent)
            {
                Logger.LogInformation("Processing minimal payload event {CmsEvent} for entity with ID {Id}", mapped.CmsEvent, mapped.IncomingEntity.Id);
                await ProcessEntityRemovalEvent(mapped, cancellationToken);
            }
            else if (!mapped.IsValid)
            {
                Logger.LogWarning("Entity {MappedEntityType} is invalid", mapped.IncomingEntity?.GetType());
            }
            else
            {
                UpsertEntity(mapped);
                await DbSaveChanges(cancellationToken);
            }

            await messageActions.CompleteMessageAsync(message, cancellationToken);
            await cacheHandler.RequestCacheClear(cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or CmsEventException)
        {
            Logger.LogError(ex, "Error processing message ID {Message}", message.MessageId);
            await messageActions.DeadLetterMessageAsync(message, null, ex.Message, ex.StackTrace, cancellationToken);
        }
        catch (Exception ex)
        {
            var retryRequired = await messageRetryHandler.RetryRequired(message, cancellationToken);

            if (retryRequired)
            {
                Logger.LogWarning(ex, "Error processing message ID {Message}will retry again", message.MessageId);
                await messageActions.CompleteMessageAsync(message, cancellationToken);
                return;
            }

            Logger.LogError(ex, "Error processing message ID {Message}, The maximum delivery count has been reached", message.MessageId);
            await messageActions.DeadLetterMessageAsync(message, null, ex.Message, ex.StackTrace, cancellationToken);
        }
    }

    public virtual Task<int> ProcessEntityRemovalEvent(MappedEntity mapped, CancellationToken cancellationToken)
    {
        if (mapped.ExistingEntity == null)
        {
            throw new InvalidOperationException("ExistingEntity is null for removal event but various validations should have prevented this.");
        }

        return db.SetComponentPublishedAndDeletedStatuses(mapped.ExistingEntity, mapped.IncomingEntity.Published, mapped.IncomingEntity.Deleted, cancellationToken);
    }

    /// <summary>
    /// Checks if the message with the given CmsEvent should be ignored based on certain conditions.
    /// </summary>
    /// <remarks>
    /// If we receive a create event, we return true.
    /// If we are NOT using preview mode (i.e. we are ignoring drafts), and the event is just a save or autosave, then return true.
    /// 
    /// Else, we return false.
    /// </remarks>
    /// <param name="cmsEvent"></param>
    /// <param name="isPublished"></param>
    /// <returns></returns>
    private bool ShouldIgnoreMessage(CmsEvent cmsEvent, bool isPublished)
    {
        if (cmsEvent == CmsEvent.CREATE)
        {
            Logger.LogInformation("Dropping received event {CmsEvent}", cmsEvent);
            return true;
        }

        if ((cmsEvent == CmsEvent.SAVE || cmsEvent == CmsEvent.AUTO_SAVE) && isPublished && !contentfulOptions.UsePreview)
        {
            Logger.LogInformation("Received {Event} but UsePreview is {UsePreview} - dropping message", cmsEvent,
                contentfulOptions.UsePreview);
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
        if (!Enum.TryParse(contentfulEvent.AsSpan()[(contentfulEvent.LastIndexOf('.') + 1)..], true,
                out CmsEvent cmsEvent))
        {
            throw new CmsEventException(string.Format("Cannot parse header \"{0}\" into a valid CMS event",
                contentfulEvent));
        }

        Logger.LogInformation("CMS Event: {CmsEvent}", cmsEvent);

        return cmsEvent;
    }

    /// <summary>
    /// Maps the incoming message to the appropriate <see cref="ContentComponentDbEntity"/>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private Task<MappedEntity> MapMessageToEntity(ServiceBusReceivedMessage message, CmsEvent cmsEvent,
        CancellationToken cancellationToken)
    {
        string messageBody = Encoding.UTF8.GetString(message.Body);

        Logger.LogInformation("Processing = {MessageBody}", messageBody);

        return mappers.ToEntity(messageBody, cmsEvent, cancellationToken);
    }

    /// <summary>
    /// Either add the entity to DB, or update the properties of the existing entity
    /// </summary>
    /// <param name="cmsEvent"></param>
    /// <param name="mapped"></param>
    /// <param name="existing"></param>
    private void UpsertEntity(MappedEntity mappedEntity)
    {
        if (!mappedEntity.AlreadyExistsInDatabase)
        {
            db.Add(mappedEntity.IncomingEntity);
        }
        else
        {
            db.Update(mappedEntity.ExistingEntity!);
        }
    }

    /// <summary>
    /// Saves changes in database, and logs information about rows changed
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task DbSaveChanges(CancellationToken cancellationToken)
    {
        long rowsChangedInDatabase = await db.SaveChangesAsync(cancellationToken);

        if (rowsChangedInDatabase == 0L)
        {
            Logger.LogWarning("No rows changed in the database!");
        }
        else
        {
            Logger.LogInformation("Updated {RowsChangedInDatabase} rows in the database", rowsChangedInDatabase);
        }
    }
}
