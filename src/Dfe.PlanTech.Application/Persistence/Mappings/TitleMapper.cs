using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class TitleMapper(EntityUpdater updater,
                         ILogger<TitleMapper> logger,
                         JsonSerializerOptions jsonSerialiserOptions,
                         IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<TitleDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
