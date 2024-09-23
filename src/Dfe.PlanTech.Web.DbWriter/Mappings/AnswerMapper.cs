using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class AnswerMapper(EntityRetriever retriever, EntityUpdater updater, ILogger<JsonToDbMapper<AnswerDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<AnswerDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "nextQuestion", "nextQuestionId");

        return values;
    }
}
