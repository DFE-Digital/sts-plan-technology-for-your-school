using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class InsetTextMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<InsetTextMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<InsetTextDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}