using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Caching.Services;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Web.DbWriter.Mappings;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Commands;

public class WebhookToDbCommand(
    CmsDbContext db,
    ICacheHandler cacheHandler,
    ContentfulOptions contentfulOptions,
    JsonToEntityMappers mappers,
    ILogger<WebhookToDbCommand> logger)
{
    public async Task<IServiceBusResult> ProcessMessage(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        try
        {
            db.ChangeTracker.Clear();

            var cmsEvent = GetCmsEvent(message.Subject);
            MappedEntity mapped = await MapMessageToEntity(message, cmsEvent, cancellationToken);

            var isPublished = mapped.ExistingEntity?.Published ?? false;

            if (ShouldIgnoreMessage(cmsEvent, isPublished))
            {
                return new ServiceBusSuccessResult();
            }

            if (mapped.IsMinimalPayloadEvent)
            {
                logger.LogInformation("Processing minimal payload event {CmsEvent} for entity with ID {Id}", mapped.CmsEvent, mapped.IncomingEntity.Id);
                await ProcessEntityRemovalEvent(mapped, cancellationToken);
            }
            else if (!mapped.IsValid)
            {
                logger.LogWarning("Entity {MappedEntityType} is invalid", mapped.IncomingEntity?.GetType());
            }
            else
            {
                UpsertEntity(mapped);
                await DbSaveChanges(cancellationToken);
            }

            await cacheHandler.RequestCacheClear(cancellationToken);
            return new ServiceBusSuccessResult();
        }
        catch (Exception ex) when (ex is JsonException or CmsEventException)
        {
            logger.LogError(ex, "Error processing message ID {Message}", message.MessageId);
            return new ServiceBusDeadLetterResult(ex.Message, ex.StackTrace, false);
        }
        catch (Exception ex)
        {
            return new ServiceBusDeadLetterResult(ex.Message, ex.StackTrace, true);
        }
    }

    protected virtual Task<int> ProcessEntityRemovalEvent(MappedEntity mapped, CancellationToken cancellationToken)
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
            logger.LogInformation("Dropping received event {CmsEvent}", cmsEvent);
            return true;
        }

        if ((cmsEvent == CmsEvent.SAVE || cmsEvent == CmsEvent.AUTO_SAVE) && isPublished && !contentfulOptions.UsePreview)
        {
            logger.LogInformation("Received {Event} but UsePreview is {UsePreview} - dropping message", cmsEvent,
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

        logger.LogInformation("CMS Event: {CmsEvent}", cmsEvent);

        return cmsEvent;
    }

    /// <summary>
    /// Maps the incoming message to the appropriate <see cref="ContentComponentDbEntity"/>
    /// </summary>
    /// <param name="body"></param>
    /// <param name="cmsEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private Task<MappedEntity> MapMessageToEntity(ServiceBusReceivedMessage message, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        string body = Encoding.UTF8.GetString(message.Body);

        logger.LogInformation("Processing = {MessageBody}", body);

        return mappers.ToEntity(body, cmsEvent, cancellationToken);
    }

    /// <summary>
    /// Either add the entity to DB, or update the properties of the existing entity
    /// </summary>
    /// <param name="mappedEntity"></param>
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
            logger.LogWarning("No rows changed in the database!");
        }
        else
        {
            logger.LogInformation("Updated {RowsChangedInDatabase} rows in the database", rowsChangedInDatabase);
        }
    }

}
