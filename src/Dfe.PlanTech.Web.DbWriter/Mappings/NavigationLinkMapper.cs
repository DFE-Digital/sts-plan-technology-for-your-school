using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class NavigationLinkMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<NavigationLinkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<NavigationLinkDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        return values;
    }
}
