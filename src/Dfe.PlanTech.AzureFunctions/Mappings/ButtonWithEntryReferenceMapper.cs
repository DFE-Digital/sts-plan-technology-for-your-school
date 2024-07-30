using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class ButtonWithEntryReferenceMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<ButtonWithEntryReferenceMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<ButtonWithEntryReferenceDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "button", "buttonId");
        values = MoveValueToNewKey(values, "linkToEntry", "linkToEntryId");

        return values;
    }
}
