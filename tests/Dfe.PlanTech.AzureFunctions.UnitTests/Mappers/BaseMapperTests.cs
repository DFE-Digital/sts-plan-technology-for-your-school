using Dfe.PlanTech.Domain.Caching.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.AzureFunctions;

public abstract class BaseMapperTests
{
    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static Dictionary<string, object?> WrapWithLocalisation(object? toWrap, string localisation = "en-US")
    => new()
    {
        [localisation] = toWrap
    };

    protected CmsWebHookPayload CreatePayload(Dictionary<string, object?> fields, string entityId)
    {
        var asJson = JsonSerializer.Serialize(fields, JsonOptions);
        var asJsonNode = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(asJson, JsonOptions);

        var payload = new CmsWebHookPayload()
        {
            Sys = new CmsWebHookSystemDetails()
            {
                Id = entityId
            },
            Fields = asJsonNode!
        };
        return payload;
    }
}