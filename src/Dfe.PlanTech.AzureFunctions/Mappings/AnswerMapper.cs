using System.Text.Json;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class AnswerMapper : JsonToDbMapper<AnswerDbEntity>
{
  public AnswerMapper(ILogger<JsonToDbMapper<AnswerDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
  {
  }

  public override Dictionary<string, object> PerformAdditionalMapping(Dictionary<string, object> values)
  {
    if (values.TryGetValue("nextQuestion", out object? value) && value is CmsWebHookSystemDetailsInner systemDetailsInner)
    {
      values["nextQuestionId"] = systemDetailsInner.Id;
      values.Remove("nextQuestion");
    }

    return values;
  }
}