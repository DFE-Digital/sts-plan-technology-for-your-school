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
        values = MoveKeyToNewValue(values, "nextQuestion", "nextQuestionId");
        values = MoveKeyToNewValue(values, "parentQuestion", "parentQuestionId");

        return values;
    }

    /// <summary>
    /// Move the value from the current key to the new key
    /// </summary>
    /// <param name="values"></param>
    /// <param name="currentKey"></param>
    /// <param name="newKey"></param>
    /// <returns></returns>
    private static Dictionary<string, object?> MoveKeyToNewValue(Dictionary<string, object?> values, string currentKey, string newKey)
    {
        if (values.TryGetValue(currentKey, out object? value) && value is CmsWebHookSystemDetailsInner systemDetailsInner)
        {
            values[newKey] = systemDetailsInner.Id;
            values.Remove(currentKey);
        }

        return values;
    }
}