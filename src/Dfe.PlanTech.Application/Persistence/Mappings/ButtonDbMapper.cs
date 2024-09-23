using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class ButtonDbMapper(EntityUpdater updater,
                            ILogger<ButtonDbMapper> logger,
                            JsonSerializerOptions jsonSerialiserOptions,
                            IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<ButtonDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
