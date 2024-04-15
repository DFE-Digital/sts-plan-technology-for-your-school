using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class AnswersComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Text", "Maturity"])
{
  public override Task ValidateContent() => ValidateAnswers();

  public async Task ValidateAnswers()
  {
    var contentfulAnswers = _contentfulContent.GetEntriesForContentType("answer").ToList();

    if (contentfulAnswers == null || contentfulAnswers.Count == 0)
    {
      Console.WriteLine("Answers not found in Contentful export");
      return;
    }

    var databaseAnswers = await _db.Answers.ToListAsync();

    if (contentfulAnswers == null || contentfulAnswers.Count == 0)
    {
      Console.WriteLine("Answers not found in database");
      return;
    }

    foreach (var contentfulAnswer in contentfulAnswers)
    {
      ValidateAnswer(databaseAnswers, contentfulAnswer);
    }
  }

  private void ValidateAnswer(List<AnswerDbEntity> databaseAnswers, JsonNode contentfulAnswer)
  {
    var databaseAnswer = databaseAnswers.FirstOrDefault(answer => answer.Id == contentfulAnswer.GetEntryId());
    if (databaseAnswer == null)
    {
      Console.WriteLine($"Could not find matching answer in DB for {contentfulAnswer}");
      return;
    }


    var nextQuestionMatches = CompareStrings("NextQuestionId", GetId(contentfulAnswer["nextQuestion"])!, databaseAnswer.NextQuestionId);

    IEnumerable<string?> validationResults = ValidateProperties(contentfulAnswer, databaseAnswer).Append(nextQuestionMatches);

    if (validationResults.Any(result => result != null))
    {
      Console.WriteLine($"Validation failures for answer {contentfulAnswer.GetEntryId()}: \n {string.Join("\n", validationResults)}");
    }
  }
}