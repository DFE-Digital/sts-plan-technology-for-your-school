using System.Text.Json;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class QuestionMapper : JsonToDbMapper<QuestionDbEntity>
{
  private readonly CmsDbContext _db;

  public QuestionMapper(CmsDbContext db, ILogger<JsonToDbMapper<QuestionDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
  {
    _db = db;
  }

  public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
  {
    UpdateAnswersParentQuestionIds(values);

    return values;
  }

  private void UpdateAnswersParentQuestionIds(Dictionary<string, object?> values)
  {
    if (values.TryGetValue("answers", out object? answersArray) && answersArray is object[] inners)
    {
      foreach (var inner in inners)
      {
        UpdateAnswerParentQuestionId(inner);
      }

      values.Remove("answers");
    }
  }

  private void UpdateAnswerParentQuestionId(object inner)
  {
    if (inner is CmsWebHookSystemDetailsInner sys)
    {
      var answer = new AnswerDbEntity()
      {
        Id = sys.Id
      };

      _db.Answers.Attach(answer);

      answer.ParentQuestionId = Payload!.Sys.Id;
    }
  }
}