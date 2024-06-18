using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Utilities;

public static class EntityToPayload
{
  private static readonly JsonSerializerOptions jsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public static ServiceBusReceivedMessage CreateServiceBusMessage<T>(T entity, Dictionary<string, object?> fields, CmsEvent cmsEvent, ILogger logger)
  where T : ContentComponent
  {
    var payload = ConvertEntityToPayload(entity, fields);

    logger.LogInformation("Created JSON payload {Payload} from entity {Entity}", payload, JsonSerializer.Serialize(entity));

    var subject = "ContentManagement.Entry." + cmsEvent.ToString().ToLower();
    var serviceBusMessage = new ServiceBusMessage(payload) { Subject = subject };

    var receivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes("")));
    return receivedMessage;
  }

  public static string ConvertEntityToPayload<T>(T entity, Dictionary<string, object?> fields)
    where T : ContentComponent
  {
    var system = new CmsWebHookSystemDetails()
    {
      Id = entity.Sys.Id,
      Type = "Entry",
      ContentType = new CmsWebHookSystemDetailsInnerContainer()
      {
        Sys = new CmsWebHookSystemDetailsInner()
        {
          Id = entity.GetType().Name,

        }
      }
    };

    var asJsonNode = WrapFields(fields);

    var payload = new CmsWebHookPayload()
    {
      Sys = system,
      Fields = asJsonNode!
    };

    return JsonSerializer.Serialize(payload, jsonOptions);
  }

  private static Dictionary<string, JsonNode> WrapFields(Dictionary<string, object?> fields)
  {
    var payloadFields = new Dictionary<string, object?>();

    foreach (var field in fields)
    {
      payloadFields[field.Key] = WrapWithLocalisation(field.Value);
    }

    var asJson = JsonSerializer.Serialize(payloadFields, jsonOptions);
    var asJsonNode = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(asJson, jsonOptions);
    return asJsonNode ?? throw new Exception("Could not convert fields");
  }

  public static Dictionary<string, object?> WrapWithLocalisation(object? toWrap, string localisation = "en-US")
  => new() { [localisation] = toWrap };
}