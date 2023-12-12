using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class TextBodyMapper : JsonToDbMapper<TextBodyDbEntity>
{
    private readonly RichTextContentMapper _richTextMapper;

    public TextBodyMapper(RichTextContentMapper richTextMapper, ILogger<TextBodyMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
        _richTextMapper = richTextMapper;
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var richText = (values["richText"] ?? throw new Exception($"No rich text value found")) as JsonNode;

        var deserialised = richText.Deserialize<RichTextContent>(JsonOptions) ?? throw new Exception($"Could not map to {typeof(RichTextContent)}");

        values["richText"] = JsonNode.Parse(JsonSerializer.Serialize(_richTextMapper.MapToDbEntity(deserialised), JsonOptions));

        return values;
    }
}