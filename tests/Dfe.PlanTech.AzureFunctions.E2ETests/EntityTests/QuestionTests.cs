
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class QuestionTests() : EntityTests<Question, QuestionDbEntity, QuestionGenerator>
{
    protected override QuestionGenerator CreateEntityGenerator() => QuestionGenerator.CreateInstance(Db);

    protected override void ClearDatabase()
    {
        var answers = Db.Answers.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
        Db.Answers.RemoveRange(answers);
        Db.SaveChanges();

        var questions = GetDbEntitiesQuery().ToList();
        Db.Questions.RemoveRange(questions);
        Db.SaveChanges();
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(Question entity)
     => new()
     {
         ["text"] = entity.Text,
         ["helpText"] = entity.HelpText,
         ["answers"] = entity.Answers.Select(answer => new { Sys = new { answer.Sys.Id } }),
         ["slug"] = entity.Slug
     };

    protected override IQueryable<QuestionDbEntity> GetDbEntitiesQuery()
    {
        Db.ChangeTracker.Clear();

        var questions = Db.Questions.IgnoreAutoIncludes()
                                    .IgnoreQueryFilters()
                                    .Select(question => new QuestionDbEntity()
                                    {
                                        Id = question.Id,
                                        Text = question.Text,
                                        HelpText = question.HelpText,
                                        Published = question.Published,
                                        Archived = question.Archived,
                                        Deleted = question.Deleted,
                                        Answers = question.Answers
                                    });

        return questions;
    }

    protected override void ValidateDbMatches(Question entity, QuestionDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.Text, dbEntity.Text);
        Assert.Equal(entity.HelpText, dbEntity.HelpText);

        ValidateEntityState(dbEntity, published, archived, deleted);

        var questionAnswers = Db.Answers.Where(answer => answer.ParentQuestionId == entity.Sys.Id).ToList();

        Assert.Equal(entity.Answers.Count, questionAnswers.Count);

        foreach (var answer in entity.Answers)
        {
            var matchingAnswer = questionAnswers.FirstOrDefault(dbAnswer => dbAnswer.Id == answer.Sys.Id);
            Assert.NotNull(matchingAnswer);
        }
    }
}