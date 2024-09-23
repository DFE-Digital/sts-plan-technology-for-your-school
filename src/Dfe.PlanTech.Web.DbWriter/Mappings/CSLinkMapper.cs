using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;
public class CSLinkMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<CSLinkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<CSLinkDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
}
