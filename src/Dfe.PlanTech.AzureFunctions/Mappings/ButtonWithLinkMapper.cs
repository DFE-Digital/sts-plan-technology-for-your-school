using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class ButtonWithLinkMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<ButtonWithLinkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<ButtonWithLinkDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "button", "buttonId");

        return values;
    }
}
