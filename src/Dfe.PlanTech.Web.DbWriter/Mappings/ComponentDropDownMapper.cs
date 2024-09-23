using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class ComponentDropDownMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<JsonToDbMapper<ComponentDropDownDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<ComponentDropDownDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{

}
