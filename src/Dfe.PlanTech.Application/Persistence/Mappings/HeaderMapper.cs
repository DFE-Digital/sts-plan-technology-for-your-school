using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class HeaderMapper(EntityUpdater updater,
                          ILogger<JsonToDbMapper<HeaderDbEntity>> logger,
                          JsonSerializerOptions jsonSerialiserOptions,
                          IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<HeaderDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{

}
