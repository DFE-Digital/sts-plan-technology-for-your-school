using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class ButtonWithEntryReferenceMapper : JsonToDbMapper<ButtonWithEntryReferenceDbEntity>
{
    public ButtonWithEntryReferenceMapper(ILogger<ButtonWithEntryReferenceMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "button", "buttonId");
        values = MoveValueToNewKey(values, "linkToEntry", "linkToEntryId");

        return values;
    }
}