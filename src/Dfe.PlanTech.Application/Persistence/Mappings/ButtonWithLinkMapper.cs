using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class ButtonWithLinkMapper(EntityUpdater updater,
                                  ILogger<ButtonWithLinkMapper> logger,
                                  JsonSerializerOptions jsonSerialiserOptions,
                                  IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<ButtonWithLinkDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "button", "buttonId");

        return values;
    }
}
