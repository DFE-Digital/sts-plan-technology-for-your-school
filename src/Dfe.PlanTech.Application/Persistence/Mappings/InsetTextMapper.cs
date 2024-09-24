using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class InsetTextMapper(EntityUpdater updater,
                             ILogger<InsetTextMapper> logger,
                             JsonSerializerOptions jsonSerialiserOptions,
                             IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<InsetTextDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
