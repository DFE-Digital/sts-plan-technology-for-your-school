using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class ButtonWithLinkMapper : JsonToDbMapper<ButtonWithLinkDbEntity>
{
    public ButtonWithLinkMapper(ILogger<ButtonWithLinkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "button", "buttonId");

        return values;
    }
}