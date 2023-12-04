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

  public override AnswerDbEntity MapEntityFields(CmsWebHookPayload payload, AnswerDbEntity entity)
  {
    var testing = new AnswerDbEntity();

    var fields = GetEntityFields(payload).Where(kvp => kvp.HasValue)
                                        .Select(kvp => kvp!.Value)
                                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    if (fields.TryGetValue("nextQuestion", out object? value) && value is CmsWebHookSystemDetailsInner systemDetailsInner)
    {
      fields["nextQuestionId"] = systemDetailsInner.Id;
      fields.Remove("nextQuestion");
    }

    var asJson = JsonSerializer.Serialize(fields);
    testing = JsonSerializer.Deserialize<AnswerDbEntity>(asJson, JsonOptions);

    return testing ?? throw new Exception("Null");
  }
}