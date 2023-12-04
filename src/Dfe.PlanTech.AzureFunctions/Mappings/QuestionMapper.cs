using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class QuestionMapper : JsonToDbMapper<QuestionDbEntity>
{
  public QuestionMapper(ILogger<JsonToDbMapper<QuestionDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
  {
  }

  public override Dictionary<string, object> PerformAdditionalMapping(Dictionary<string, object> values)
  {
    values.Remove("answers");
    return values;
  }
}