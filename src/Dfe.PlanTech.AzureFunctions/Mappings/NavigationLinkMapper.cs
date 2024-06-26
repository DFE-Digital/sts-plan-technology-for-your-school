using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class NavigationLinkMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<NavigationLinkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<NavigationLinkDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}