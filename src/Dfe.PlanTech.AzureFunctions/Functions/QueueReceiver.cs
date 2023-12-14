using System.Text;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
                string cmsEvent = message.Subject.AsSpan()[(message.Subject.LastIndexOf('.') + 1)..].ToString();
                var text = Encoding.UTF8.GetString(message.Body);

                Logger.LogInformation("Performing Action: {action}", cmsEvent);
                Logger.LogInformation("Processing {text}", text);

                ContentComponentDbEntity mapped = _mappers.ToEntity(text);
                ContentComponentDbEntity? existing = GetExistingDbEntity(mapped);

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
                        // TODO: Probably something more appropriate than ArgumentException? Custom?
                        throw new ArgumentException(string.Format("Case \"{0}\" not implemented", cmsEvent));
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

        private ContentComponentDbEntity? GetExistingDbEntity(ContentComponentDbEntity entity)
        {
            return _db.Find(entity.GetType(), entity.Id) as ContentComponentDbEntity ?? null;
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

            foreach (var property in properties)
            {
                property.SetValue(existing, property.GetValue(entity));
            }
        }
    }
}
