using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class AnswersComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Text", "Maturity"], "Answer")
{
    public override Task ValidateContent()
    {
        ValidateAnswers();

        return Task.CompletedTask;
    }

    public void ValidateAnswers()
    {
        foreach (var contentfulAnswer in _contentfulEntities)
        {
            ValidateAnswer(_dbEntities.OfType<AnswerDbEntity>().ToArray(), contentfulAnswer);
        }
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.Answers;
    }

    private void ValidateAnswer(AnswerDbEntity[] databaseAnswers, JsonNode contentfulAnswer)
    {
        var databaseAnswer = TryRetrieveMatchingDbEntity(databaseAnswers, contentfulAnswer);
        if (databaseAnswer == null)
        {
            return;
        }

        var nextQuestionMatches = CompareStrings(contentfulAnswer["nextQuestion"]?.GetEntryId(), databaseAnswer.NextQuestionId);

        var nextQustionError = nextQuestionMatches != null ? GenerateDataValidationError("NextQuestionId", nextQuestionMatches) : null;

        ValidateProperties(contentfulAnswer, databaseAnswer, nextQustionError);
    }
}
