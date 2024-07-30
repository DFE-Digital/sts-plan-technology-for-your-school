using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class QuestionsComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Text", "HelpText", "Slug"], "Question")
{
    public override Task ValidateContent()
    {
        ValidateQuestions();
        return Task.CompletedTask;
    }

    public void ValidateQuestions()
    {
        foreach (var contentfulQuestion in _contentfulEntities)
        {
            ValidateQuestion(_dbEntities.OfType<QuestionDbEntity>().ToArray(), contentfulQuestion);
        }
    }

    private void ValidateQuestion(QuestionDbEntity[] databaseQuestions, JsonNode contentfulQuestion)
    {
        var databaseQuestion = TryRetrieveMatchingDbEntity(databaseQuestions, contentfulQuestion);
        if (databaseQuestion == null)
        {
            return;
        }

        ValidateProperties(contentfulQuestion, databaseQuestion!, ValidateChildren(contentfulQuestion, "answers", databaseQuestion, dbQuestion => dbQuestion.Answers).ToArray());
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.Questions.Select(question => new QuestionDbEntity()
        {
            Id = question.Id,
            Text = question.Text,
            HelpText = question.HelpText,
            Slug = question.Slug,
            Answers = question.Answers.Select(answer => new AnswerDbEntity()
            {
                Id = answer.Id
            }).ToList()
        });
    }
}
