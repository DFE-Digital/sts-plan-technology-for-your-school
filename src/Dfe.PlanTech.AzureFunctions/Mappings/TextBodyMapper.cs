using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class TextBodyMapper(EntityRetriever retriever, EntityUpdater updater, RichTextContentMapper richTextMapper, ILogger<TextBodyMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<TextBodyDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly RichTextContentMapper _richTextMapper = richTextMapper;

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var richText = (values["richText"] ?? throw new KeyNotFoundException($"No rich text value found")) as JsonNode;

        var deserialised = richText.Deserialize<RichTextContent>(JsonOptions) ?? throw new InvalidOperationException($"Could not map to {typeof(RichTextContent)}");

        values["richText"] = JsonNode.Parse(JsonSerializer.Serialize(_richTextMapper.MapToDbEntity(deserialised), JsonOptions));

        return values;
    }
}
