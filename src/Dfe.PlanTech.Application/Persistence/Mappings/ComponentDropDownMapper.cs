using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class ComponentDropDownMapper(EntityUpdater updater,
                                     ILogger<JsonToDbMapper<ComponentDropDownDbEntity>> logger,
                                     JsonSerializerOptions jsonSerialiserOptions,
                                     IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<ComponentDropDownDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{

}
