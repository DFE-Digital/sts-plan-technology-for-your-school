using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class NavigationLinkMapper(EntityUpdater updater,
                                  ILogger<NavigationLinkMapper> logger,
                                  JsonSerializerOptions jsonSerialiserOptions,
                                  IDatabaseHelper<ICmsDbContext> databaseHelper) : BaseJsonToDbMapper<NavigationLinkDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
