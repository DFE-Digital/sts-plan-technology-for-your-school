using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class TitleMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<TitleMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<TitleDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}