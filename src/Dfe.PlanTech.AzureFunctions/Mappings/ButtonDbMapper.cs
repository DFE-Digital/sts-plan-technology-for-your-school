using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class ButtonDbMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<ButtonDbMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<ButtonDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
}