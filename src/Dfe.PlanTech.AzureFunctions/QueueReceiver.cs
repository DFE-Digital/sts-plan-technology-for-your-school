using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Auth;
using Dfe.PlanTech.AzureFunctions.ServiceBus;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions
{
  public class QueueReceiver
  {
    private readonly ILogger _logger;
    private readonly CmsDbContext _db;
    private readonly JsonSerializerOptions _jsonSerialiserOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true
    };

    public QueueReceiver(ILoggerFactory loggerFactory, CmsDbContext db)
    {
      _logger = loggerFactory.CreateLogger<QueueReceiver>();
      _db = db;
    }

    [Function("QueueReceiver")]
    public async Task QueueReceiverDbWriter([ServiceBusTrigger("contentful", IsBatched = true)] ServiceBusReceivedMessage[] messages, ServiceBusMessageActions messageActions)
    {
      _logger.LogInformation("Queue Receiver -> Db Writer started");

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
        _logger.LogInformation("Processing {text}", text);

        var payload = JsonSerializer.Deserialize<CmsWebHookPayload>(text, _jsonSerialiserOptions);

        if (payload == null)
        {
          _logger.LogError("Could not serialise value to expected payload. Value was {0}", text);
          await messageActions.DeadLetterMessageAsync(message);
        }

        _logger.LogInformation("Payload ID is {id}", payload!.Sys.Id);

        var existingEntity = await _db.ContentJson.FirstOrDefaultAsync(json => json.ContentId == payload!.Sys.Id);

        if (existingEntity != null)
        {
          existingEntity.ContentJson = text;
        }
        else
        {
          var entity = new JsonCmsDbEntity()
          {
            ContentTypeId = payload!.Sys.ContentType.Sys.Id,
            ContentJson = text,
            ContentId = payload!.Sys.Id,
            IsPublished = true
          };

          _db.ContentJson.Add(entity);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Wrote entity");

        await messageActions.CompleteMessageAsync(message);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
        await messageActions.DeadLetterMessageAsync(message);
      }
    }
  }
}
