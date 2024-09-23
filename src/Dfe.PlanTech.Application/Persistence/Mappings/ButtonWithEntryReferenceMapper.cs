using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class ButtonWithEntryReferenceMapper(EntityUpdater updater,
                                            ILogger<ButtonWithEntryReferenceMapper> logger,
                                            JsonSerializerOptions jsonSerialiserOptions,
                                            IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<ButtonWithEntryReferenceDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "button", "buttonId");
        values = MoveValueToNewKey(values, "linkToEntry", "linkToEntryId");

        return values;
    }
}
