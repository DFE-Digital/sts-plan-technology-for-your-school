using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Utilities;


public static class EntityToPayload
{
    public static object ToEntityId(this ContentComponent contentComponent)
    => new { Sys = new { contentComponent.Sys.Id } };

    public static IEnumerable<object> ToEntityIds(this IEnumerable<ContentComponent> contentComponents)
    => contentComponents.Select(ToEntityId);

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
