using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class QuestionsComparatorclass(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Text", "HelpText", "Slug"], "Question")
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
    var databaseQuestion = FindMatchingDbEntity(databaseQuestions, contentfulQuestion);
    if (databaseQuestion == null)
    {
      return;
    }

    ValidateProperties(contentfulQuestion, databaseQuestion!);

    var contentfulQuestionAnswerIds = contentfulQuestion["answers"]?.AsArray().Select(GetId).Where(id => id != null).ToArray();

    if (contentfulQuestionAnswerIds == null || contentfulQuestionAnswerIds.Length == 0)
    {
      Console.WriteLine("No answers found for question");
      return;
    }

    CheckForMissingAnswers(contentfulQuestion, databaseQuestion, contentfulQuestionAnswerIds);

    CheckForExtraAnswers(contentfulQuestion, databaseQuestion, contentfulQuestionAnswerIds);
  }

  private static void CheckForExtraAnswers(JsonNode contentfulQuestion, QuestionDbEntity databaseQuestion, string?[] contentfulQuestionAnswerIds)
  {
    var extraDbAnswers = string.Join("\n", databaseQuestion.Answers.Where(answer => ValidateDbAnswerExistsInContentful(answer, contentfulQuestionAnswerIds)));

    if (!string.IsNullOrEmpty(extraDbAnswers))
    {
      Console.WriteLine($"DB has extra answers for question {contentfulQuestion.GetEntryId()}: \n {extraDbAnswers}");
    }
  }

  private static void CheckForMissingAnswers(JsonNode contentfulQuestion, QuestionDbEntity databaseQuestion, string?[] contentfulQuestionAnswerIds)
  {
    var missingDbAnswers = string.Join("\n", contentfulQuestionAnswerIds.Where(answerId => ValidateContentfulAnswerExistsInDb(databaseQuestion, answerId!)));

    if (!string.IsNullOrEmpty(missingDbAnswers))
    {
      Console.WriteLine($"Missing answers for question {contentfulQuestion.GetEntryId()}: \n {missingDbAnswers}");
    }
  }

  private static bool ValidateDbAnswerExistsInContentful(AnswerDbEntity answer, string?[] contentfulQuestionAnswerIds)
    => !contentfulQuestionAnswerIds.Any(answerId => answer.Id == answerId);

  private static bool ValidateContentfulAnswerExistsInDb(QuestionDbEntity? databaseQuestion, string answerId)
    => !databaseQuestion!.Answers.Exists(answer => answer.Id == answerId);

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