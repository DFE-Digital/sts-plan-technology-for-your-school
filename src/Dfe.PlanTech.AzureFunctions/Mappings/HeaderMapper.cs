using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class HeaderMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<JsonToDbMapper<HeaderDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<HeaderDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{

}