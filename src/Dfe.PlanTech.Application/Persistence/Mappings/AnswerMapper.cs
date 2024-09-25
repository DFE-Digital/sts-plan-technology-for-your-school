using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class AnswerMapper(EntityUpdater updater,
                          ILogger<AnswerMapper> logger,
                          JsonSerializerOptions jsonSerialiserOptions,
                          IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<AnswerDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "nextQuestion", "nextQuestionId");

        return values;
    }
}
