using System.Text.Json;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class AnswerMapper(EntityUpdater updater,
                          ILogger<JsonToDbMapper<AnswerDbEntity>> logger,
                          JsonSerializerOptions jsonSerialiserOptions,
                          IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<AnswerDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "nextQuestion", "nextQuestionId");

        return values;
    }
}
