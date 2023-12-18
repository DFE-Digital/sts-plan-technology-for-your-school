using System.Text;
using System.Text.Json;
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
                var text = Encoding.UTF8.GetString(message.Body);
                Logger.LogInformation("Processing {text}", text);

                var mapped = _mappers.ToEntity(text);

                var rowsChanged = await UpsertEntityInDatabase(mapped);

                if (rowsChanged == 0)
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

        private async Task<long> UpsertEntityInDatabase(ContentComponentDbEntity entity)
        {
            var existing = _db.Find(entity.GetType(), entity.Id);

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

        private static void UpdateProperties(ContentComponentDbEntity entity, object? existing)
        {
            var properties = entity.GetType().GetProperties();

            foreach (var property in properties)
            {
                property.SetValue(existing, property.GetValue(entity));
            }
        }
    }
}

