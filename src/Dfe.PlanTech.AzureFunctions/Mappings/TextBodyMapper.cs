using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class TextBodyMapper : JsonToDbMapper<TextBodyDbEntity>
{
  private readonly JsonToDbMapper<RichTextContentDbEntity> _richTextMapper;

  public TextBodyMapper(JsonToDbMapper<RichTextContentDbEntity> richTextMapper, ILogger<TextBodyMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
  {
    _richTextMapper = richTextMapper;
  }

  public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
  {
    var richText = (values["richText"] ?? throw new Exception($"No rich text value found")) as JsonNode;

    var deserialised = richText.Deserialize<RichTextContent>(JsonOptions);


  }

    return values;
  }
}