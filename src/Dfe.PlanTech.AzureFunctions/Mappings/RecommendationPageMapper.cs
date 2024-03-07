using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationPageMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<RecommendationPageMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<RecommendationPageDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "page", "pageId");

        return values;
    }
}