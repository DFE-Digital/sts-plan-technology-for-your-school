using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;

// TODO: Notes for today!!!
// 1. Rejig all this so mapping, updating, adding is all separate methods
// 2. To deal with unpublished, essentially do 'if existing entity exists && not unpublished (do value copy)'
// 3. And changed mapped.Published = False to existing.Published = False
// 4. Because EF Core tracks 'existing' and understands that only that property has changed
// 5. So the section will be unmodified, but the ContentComponent will get changed

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
    public async Task QueueReceiverDbWriter([ServiceBusTrigger("contentful", IsBatched = true)] ServiceBusReceivedMessage[] messages, ServiceBusMessageActions messageActions)
    {
        Logger.LogInformation("Queue Receiver -> Db Writer started. Processing {msgCount} messages", messages.Length);

        foreach (ServiceBusReceivedMessage message in messages)
        {
            await ProcessMessage(message, messageActions);
        }
    }

    private async Task ProcessMessage(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            string cmsEvent = GetCmsEvent(message.Subject);

            ContentComponentDbEntity mapped = GetMapped(message);
            ContentComponentDbEntity? existing = await GetExisting(mapped);

            if (cmsEvent.Equals("unpublish") && existing != null)
            {
                existing.Published = false;
            }
            else
            {
                ProcessCmsEvent(cmsEvent, ref mapped);

                if (existing == null)
                {
                    DbAdd(mapped);
                }
                else
                {
                    DbUpdate(mapped, existing);
                }
            }

            await DbSaveChanges();

            await messageActions.CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
            await messageActions.DeadLetterMessageAsync(message);
        }
    }

    private static string GetCmsEvent(string subject)
    {
        return subject.AsSpan()[(subject.LastIndexOf('.') + 1)..].ToString();
    }

    private ContentComponentDbEntity GetMapped(ServiceBusReceivedMessage message)
    {
        string messageBody = Encoding.UTF8.GetString(message.Body);

        Logger.LogInformation("Processing = {messageBody}", messageBody);

        return _mappers.ToEntity(messageBody);
    }

    private async Task<ContentComponentDbEntity?> GetExisting(ContentComponentDbEntity mapped)
    {
        ContentComponentDbEntity? existing = await GetExistingDbEntity(mapped);

        if (existing != null)
        {
            mapped.Archived = existing.Archived;
            mapped.Published = existing.Published;
            mapped.Deleted = existing.Deleted;
        }

        return existing;
    }

    private async Task<ContentComponentDbEntity?> GetExistingDbEntity(ContentComponentDbEntity entity)
    {
        var model = _db.Model.FindEntityType(entity.GetType()) ?? throw new Exception($"Could not find model in database for {entity.GetType()}");

        var dbSet = GetIQueryableForEntity(model);

        var found = await dbSet.IgnoreAutoIncludes()
                                .FirstOrDefaultAsync(existing => existing.Id == entity.Id);

        return found ?? null;
    }

    private IQueryable<ContentComponentDbEntity> GetIQueryableForEntity(IEntityType model)
    => (IQueryable<ContentComponentDbEntity>)_db
                                    .GetType()
                                    .GetMethod("Set", 1, Type.EmptyTypes)!
                                    .MakeGenericMethod(model!.ClrType)!
                                    .Invoke(_db, null)!;

    private static void ProcessCmsEvent(string cmsEvent, ref ContentComponentDbEntity mapped)
    {
        switch (cmsEvent)
        {
            case "create":
            case "save":
            case "auto_save":
                break;
            case "archive":
                mapped.Archived = true;
                break;
            case "unarchive":
                mapped.Archived = false;
                break;
            case "publish":
                mapped.Published = true;
                break;
            case "unpublish":
                mapped.Published = false;
                break;
            case "delete":
                mapped.Deleted = true;
                break;
            default:
                throw new CmsEventException(string.Format("CMS Event \"{0}\" not implemented", cmsEvent));
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
            .Where(property => !property.Name.EndsWith("Id") && !HasDontCopyValueAttribute(property));

    /// <summary>
    /// Does the property have a <see cref="DontCopyValueAttribute"/> property attached to it? 
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private bool HasDontCopyValueAttribute(PropertyInfo property)
     => property.CustomAttributes.Any(attribute => attribute.AttributeType == _dontCopyValueAttribute);

    private async Task DbSaveChanges()
    {
        long rowsChangedInDatabase = await _db.SaveChangesAsync();

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