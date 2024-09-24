using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class ButtonWithLinkMapper(EntityUpdater updater,
                                  ILogger<ButtonWithLinkMapper> logger,
                                  JsonSerializerOptions jsonSerialiserOptions,
                                  IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<ButtonWithLinkDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "button", "buttonId");

        return values;
    }
}
