using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class WarningComponentMapper : JsonToDbMapper<WarningComponentDbEntity>
{
    public WarningComponentMapper(ILogger<WarningComponentMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "text", "textId");

        return values;
    }
}