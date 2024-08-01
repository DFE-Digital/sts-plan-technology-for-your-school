using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;
public class CSLinkMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<CSLinkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<CSLinkDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
}
