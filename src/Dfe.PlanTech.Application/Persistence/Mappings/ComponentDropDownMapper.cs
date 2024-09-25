using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class ComponentDropDownMapper(EntityUpdater updater,
                                     ILogger<JsonToDbMapper<ComponentDropDownDbEntity>> logger,
                                     JsonSerializerOptions jsonSerialiserOptions,
                                     IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<ComponentDropDownDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{

}
