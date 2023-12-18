using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class AnswerMapper : JsonToDbMapper<AnswerDbEntity>
{
    public AnswerMapper(ILogger<JsonToDbMapper<AnswerDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "nextQuestion", "nextQuestionId");

        return values;
    }
}