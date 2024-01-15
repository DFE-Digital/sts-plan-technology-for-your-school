using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Dfe.PlanTech.AzureFunctions
{
    public class QueueReceiver : BaseFunction
    {
        private readonly CmsDbContext _db;
        private readonly JsonToEntityMappers _mappers;

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
                var text = Encoding.UTF8.GetString(message.Body);

                Logger.LogInformation("Performing Action: {action}", cmsEvent);
                Logger.LogInformation("Processing {text}", text);

                ContentComponentDbEntity mapped = _mappers.ToEntity(text);
                ContentComponentDbEntity? existing = await GetExistingDbEntity(mapped);

                if (existing != null)
                {
                    mapped.Archived = existing.Archived;
                    mapped.Published = existing.Published;
                    mapped.Deleted = existing.Deleted;
                }

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

                long rowsChanged = await UpsertEntityInDatabase(mapped, existing);

                if (rowsChanged == 0L)
                {
                    Logger.LogError("Changed no rows in database");
                }
                else
                {
                    Logger.LogInformation($"Updated {rowsChanged} rows in the database");
                }

                await messageActions.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await messageActions.DeadLetterMessageAsync(message);
            }
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

        private static string GetCmsEvent(string subject)
        {
            return subject.AsSpan()[(subject.LastIndexOf('.') + 1)..].ToString();
        }

        private async Task<long> UpsertEntityInDatabase(ContentComponentDbEntity entity, ContentComponentDbEntity? existing)
        {
            if (existing == null)
            {
                _db.Add(entity);
            }
            else
            {
                UpdateProperties(entity, existing);
            }

            return await _db.SaveChangesAsync();
        }

        private static void UpdateProperties(ContentComponentDbEntity entity, ContentComponentDbEntity existing)
        {
            var properties = entity.GetType().GetProperties();

            foreach (var property in properties.Where(property => !property.Name.EndsWith("Id")))
            {
                property.SetValue(existing, property.GetValue(entity));
            }
        }
    }
}
