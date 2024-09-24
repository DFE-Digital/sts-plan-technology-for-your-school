using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class TitleMapper(EntityUpdater updater,
                         ILogger<TitleMapper> logger,
                         JsonSerializerOptions jsonSerialiserOptions,
                         IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<TitleDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
