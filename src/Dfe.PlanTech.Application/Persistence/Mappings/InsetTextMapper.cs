using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class InsetTextMapper(EntityUpdater updater,
                             ILogger<InsetTextMapper> logger,
                             JsonSerializerOptions jsonSerialiserOptions,
                             IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<InsetTextDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
