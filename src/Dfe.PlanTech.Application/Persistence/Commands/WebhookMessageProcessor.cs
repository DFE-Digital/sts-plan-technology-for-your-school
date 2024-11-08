using System.Text.Json;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Commands;

/// <inheritdoc cref="IWebhookToDbCommand" />
public class WebhookMessageProcessor(ICmsCache cache,
                                ContentfulOptions contentfulOptions,
                                JsonSerializerOptions jsonSerialiserOptions,
                                ILogger<WebhookMessageProcessor> logger,
                                IDatabaseHelper<ICmsDbContext> databaseHelper) : IWebhookToDbCommand
{
    public async Task<IServiceBusResult> ProcessMessage(string subject, string body, string id, CancellationToken cancellationToken)
    {
        try
        {
            var payload = MapMessageToPayload(body);

            await cache.InvalidateCacheAsync(payload.Sys.Id);
            return new ServiceBusSuccessResult();
        }
        catch (Exception ex) when (ex is JsonException or CmsEventException)
        {
            logger.LogError(ex, "Error processing message ID {Message}", id);
            return new ServiceBusErrorResult(ex.Message, ex.StackTrace, false);
        }
        catch (Exception ex)
        {
            return new ServiceBusErrorResult(ex.Message, ex.StackTrace, true);
        }
    }

    protected virtual Task<int> ProcessEntityRemovalEvent(MappedEntity mapped, CancellationToken cancellationToken)
    {
        if (mapped.ExistingEntity == null)
        {
            throw new InvalidOperationException("ExistingEntity is null for removal event but various validations should have prevented this.");
        }

        return databaseHelper.Database.SetComponentPublishedAndDeletedStatuses(mapped.ExistingEntity, mapped.IncomingEntity.Published, mapped.IncomingEntity.Deleted, cancellationToken);
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
        switch (cmsEvent)
        {
            case CmsEvent.CREATE:
                logger.LogInformation("Dropping received event {CmsEvent}", cmsEvent);
                return true;
            case CmsEvent.SAVE or CmsEvent.AUTO_SAVE when isPublished && !contentfulOptions.UsePreview:
                logger.LogInformation("Received {Event} but UsePreview is {UsePreview} - dropping message", cmsEvent,
                    contentfulOptions.UsePreview);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Retrieves the CmsEvent based on the provided subject.
    /// </summary>
    /// <param name="contentfulEvent">The Contentful event (from the Service Bus message subject), e.g. "ContentManagement.Entry.publish"</param>
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
    /// Maps the incoming message to a CmsWebHookPayload
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    private CmsWebHookPayload MapMessageToPayload(string body)
    {
        logger.LogInformation("Processing mesasge:\n{MessageBody}", body);

        return JsonSerializer.Deserialize<CmsWebHookPayload>(body, jsonSerialiserOptions) ?? throw new InvalidOperationException($"Could not serialise body to {typeof(CmsWebHookPayload)}. Body was {body}");
    }

    /// <summary>
    /// Either add the entity to DB, or update the properties of the existing entity
    /// </summary>
    /// <param name="mappedEntity"></param>
    private void UpsertEntity(MappedEntity mappedEntity)
    {
        if (!mappedEntity.AlreadyExistsInDatabase)
        {
            databaseHelper.Add(mappedEntity.IncomingEntity);
        }
        else if (mappedEntity.ExistingEntity != null)
        {
            databaseHelper.Update(mappedEntity.ExistingEntity!);
        }
        else
        {
            throw new InvalidDataException($"Entity {mappedEntity.IncomingEntity.Id} does not already exist in database, but existing entity is null");
        }
    }

    /// <summary>
    /// Saves changes in database, and logs information about rows changed
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task DbSaveChanges(CancellationToken cancellationToken)
    {
        long rowsChangedInDatabase = await databaseHelper.Database.SaveChangesAsync(cancellationToken);

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
