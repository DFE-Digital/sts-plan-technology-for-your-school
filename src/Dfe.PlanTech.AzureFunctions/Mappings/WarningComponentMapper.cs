using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class WarningComponentMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<WarningComponentMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<WarningComponentDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "text", "textId");

        return values;
    }
}
