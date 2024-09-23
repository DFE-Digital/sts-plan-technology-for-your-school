using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class WarningComponentMapper(EntityUpdater updater,
                                    ILogger<WarningComponentMapper> logger,
                                    JsonSerializerOptions jsonSerialiserOptions,
                                    IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<WarningComponentDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "text", "textId");

        return values;
    }
}
