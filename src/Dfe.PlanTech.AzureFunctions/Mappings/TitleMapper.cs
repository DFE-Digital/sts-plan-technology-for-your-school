using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class TitleMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<TitleMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<TitleDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
}
