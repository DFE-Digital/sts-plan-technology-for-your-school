using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class InsetTextMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<InsetTextMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<InsetTextDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
}
