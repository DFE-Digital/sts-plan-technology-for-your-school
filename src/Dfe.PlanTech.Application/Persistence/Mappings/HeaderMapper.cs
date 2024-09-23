using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class HeaderMapper(EntityUpdater updater,
                          ILogger<JsonToDbMapper<HeaderDbEntity>> logger,
                          JsonSerializerOptions jsonSerialiserOptions,
                          IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<HeaderDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{

}
