using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
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
    private readonly JsonToEntityMappers _mappers;
    private readonly Type _dontCopyValueAttribute = typeof(DontCopyValueAttribute);

    public QueueReceiver(ILoggerFactory loggerFactory, CmsDbContext db, JsonToEntityMappers mappers) : base(loggerFactory.CreateLogger<QueueReceiver>())
    {
        _db = db;
        _mappers = mappers;
    }

    [Function("QueueReceiver")]
    public async Task QueueReceiverDbWriter([ServiceBusTrigger("contentful", IsBatched = true)] ServiceBusReceivedMessage[] messages, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Queue Receiver -> Db Writer started. Processing {msgCount} messages", messages.Length);

        foreach (ServiceBusReceivedMessage message in messages)
        {
            await ProcessMessage(message, messageActions, cancellationToken);
        }
    }

    private async Task ProcessMessage(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
    {
        try
        {
            CmsEvent cmsEvent = GetCmsEvent(message.Subject);

            ContentComponentDbEntity mapped = GetMapped(message);
            ContentComponentDbEntity? existing = await GetExisting(mapped, cancellationToken);

            ProcessCmsEvent(cmsEvent, mapped, existing);

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

    private CmsEvent GetCmsEvent(string subject)
    {
        if (!Enum.TryParse(subject.AsSpan()[(subject.LastIndexOf('.') + 1)..], true, out CmsEvent cmsEvent))
        {
            throw new CmsEventException(string.Format("Cannot parse header \"{0}\" into a valid CMS event", subject));
        }

        Logger.LogInformation("CMS Event: {cmsEvent}", cmsEvent);

        return cmsEvent;
    }

    private ContentComponentDbEntity GetMapped(ServiceBusReceivedMessage message)
    {
        string messageBody = Encoding.UTF8.GetString(message.Body);

        Logger.LogInformation("Processing = {messageBody}", messageBody);

        return _mappers.ToEntity(messageBody);
    }

    private async Task<ContentComponentDbEntity?> GetExisting(ContentComponentDbEntity mapped, CancellationToken cancellationToken)
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

    private async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity, CancellationToken cancellationToken)
    {
        var model = _db.Model.FindEntityType(entity.GetType()) ?? throw new Exception($"Could not find model in database for {entity.GetType()}");

        var dbSet = GetIQueryableForEntity(model);

        var found = await dbSet.IgnoreAutoIncludes()
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(existing => existing.Id == entity.Id, cancellationToken);

        return found ?? null;
    }

    private IQueryable<ContentComponentDbEntity> GetIQueryableForEntity(IEntityType model)
    => (IQueryable<ContentComponentDbEntity>)_db
                                    .GetType()
                                    .GetMethod("Set", 1, Type.EmptyTypes)!
                                    .MakeGenericMethod(model!.ClrType)!
                                    .Invoke(_db, null)!;

    private static void ProcessCmsEvent(CmsEvent cmsEvent, ContentComponentDbEntity mapped, ContentComponentDbEntity? existing)
    {
        switch (cmsEvent)
        {
            case CmsEvent.CREATE:
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

    private void UpsertEntity(CmsEvent cmsEvent, ContentComponentDbEntity mapped, ContentComponentDbEntity? existing)
    {
        if (cmsEvent == CmsEvent.UNPUBLISH) return;

        if (existing == null)
        {
            DbAdd(mapped);
        }
        else
        {
            DbUpdate(mapped, existing);
        }
    }

    private void DbAdd(ContentComponentDbEntity mapped)
    {
        _db.Add(mapped);
    }

    private void DbUpdate(ContentComponentDbEntity mapped, ContentComponentDbEntity existing)
    {
        UpdateProperties(mapped, existing);
    }

    private void UpdateProperties(ContentComponentDbEntity entity, ContentComponentDbEntity existing)
    {
        foreach (var property in PropertiesToCopy(entity))
        {
            property.SetValue(existing, property.GetValue(entity));
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

    private async Task DbSaveChanges(CancellationToken cancellationToken)
    {
        long rowsChangedInDatabase = await _db.SaveChangesAsync(cancellationToken);

        if (rowsChangedInDatabase == 0L)
        {
            Logger.LogError("No rows changed in the database!");
        }
        else
        {
            Logger.LogInformation($"Updated {rowsChangedInDatabase} rows in the database");
        }
    }
}