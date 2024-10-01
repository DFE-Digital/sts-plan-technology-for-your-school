using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class ButtonMapper(EntityUpdater updater,
                            ILogger<ButtonMapper> logger,
                            JsonSerializerOptions jsonSerialiserOptions,
                            IDatabaseHelper<ICmsDbContext> databaseHelper) : BaseJsonToDbMapper<ButtonDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
}
