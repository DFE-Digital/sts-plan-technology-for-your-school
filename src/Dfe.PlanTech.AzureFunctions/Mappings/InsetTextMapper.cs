using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class InsetTextMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<InsetTextMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<InsetTextDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
}
