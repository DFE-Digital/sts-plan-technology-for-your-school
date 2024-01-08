using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
        UpdateReferencesArray(values, "answers", _db.Answers, (id, answer) => answer.ParentQuestionId = Payload!.Sys.Id);
    }
}
