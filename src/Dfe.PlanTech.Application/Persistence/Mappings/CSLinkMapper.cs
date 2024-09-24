using System.Text.Json;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;
public class CSLinkMapper(EntityUpdater updater,
                          ILogger<CSLinkMapper> logger,
                          JsonSerializerOptions jsonSerialiserOptions,
                          IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<CSLinkDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
