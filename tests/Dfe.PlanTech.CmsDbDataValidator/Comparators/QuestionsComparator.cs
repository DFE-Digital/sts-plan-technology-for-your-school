using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class QuestionsComparatorclass(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Text", "HelpText", "Slug"])
{
  public override Task ValidateContent() => ValidateQuestions();

  public async Task ValidateQuestions()
  {
    var contentfulQuestions = _contentfulContent.GetEntriesForContentType("question").ToList();

    if (contentfulQuestions == null || contentfulQuestions.Count == 0)
    {
      Console.WriteLine("Questions not found in Contentful export");
      return;
    }

    var databaseQuestions = await _db.Questions.Select(question => new QuestionDbEntity()
    {
      Id = question.Id,
      Text = question.Text,
      HelpText = question.HelpText,
      Slug = question.Slug,
      Answers = question.Answers.Select(answer => new AnswerDbEntity()
      {
        Id = answer.Id
      }).ToList()
    }).ToListAsync();

    if (contentfulQuestions == null || contentfulQuestions.Count == 0)
    {
      Console.WriteLine("Questions not found in Database");
      return;
    }

    foreach (var contentfulQuestion in contentfulQuestions)
    {
      ValidateQuestion(databaseQuestions, contentfulQuestion);
    }
  }

  private void ValidateQuestion(List<QuestionDbEntity> databaseQuestions, JsonNode contentfulQuestion)
  {
    var databaseQuestion = databaseQuestions.FirstOrDefault(question => question.Id == contentfulQuestion.GetEntryId());

    if (databaseQuestion == null)
    {
      Console.WriteLine($"Could not find matching question in DB for {contentfulQuestion}");
      return;
    }
    string?[] validationResults = ValidateProperties(contentfulQuestion, databaseQuestion!).ToArray();

    LogValidationMessages("Question", validationResults, contentfulQuestion);

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

  private static string? ValidateDbAnswerExistsInContentful(QuestionDbEntity? databaseQuestion, JsonNode questionAnswer)
  {
    var answerId = questionAnswer!["sys"]?["id"]?.GetValue<string>() ?? throw new JsonException($"Couldn't find Id in {questionAnswer}");

    var exists = databaseQuestion!.Answers.Exists(answer => answer.Id == answerId);

    return exists ? null : answerId;
  }

}