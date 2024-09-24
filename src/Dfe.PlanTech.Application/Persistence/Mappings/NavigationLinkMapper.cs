using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class NavigationLinkMapper(EntityUpdater updater,
                                  ILogger<NavigationLinkMapper> logger,
                                  JsonSerializerOptions jsonSerialiserOptions,
                                  IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<NavigationLinkDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
