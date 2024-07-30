using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dfe.PlanTech.CmsDbDataValidator;

public static class ContentfulHelpers
{
    public static string? GetEntryId(this JsonNode jsonNode) => (jsonNode["sys"]?["id"] ?? jsonNode["id"])?.GetValue<string>();

    public static JsonNode WithoutLocalisation(this JsonNode jsonNode)
    {
        var inner = jsonNode["en-US"] ?? throw new JsonException($"No localisation found for {jsonNode}");

        return inner.Deserialize<JsonNode>()!;
    }
}
