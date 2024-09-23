using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class TextBodyMapper(EntityUpdater updater,
                            RichTextContentMapper richTextMapper,
                            ILogger<TextBodyMapper> logger,
                            JsonSerializerOptions jsonSerialiserOptions,
                            IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<TextBodyDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private readonly RichTextContentMapper _richTextMapper = richTextMapper;

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        if (values.TryGetValue("richText", out object? richTextValue) && richTextValue is JsonNode richText)
        {
            var deserialised = richText.Deserialize<RichTextContent>(JsonOptions) ?? throw new InvalidOperationException($"Could not map to {typeof(RichTextContent)}");

            values["richText"] = JsonNode.Parse(JsonSerializer.Serialize(_richTextMapper.MapToDbEntity(deserialised), JsonOptions));
        }

        return values;
    }
}
