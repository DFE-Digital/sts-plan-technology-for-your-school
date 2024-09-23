using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class ButtonDbMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<ButtonDbMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<ButtonDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
}
