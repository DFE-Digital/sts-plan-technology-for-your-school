using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dfe.PlanTech.DataValidation.Tests;

public class CachedDataComparisonTests
{
  private readonly CmsDbContext _db;
  private readonly ContentfulContent _contentfulContent = new ContentfulContent("contentful-export.json");

  public CachedDataComparisonTests()
  {
    _db = DatabaseHelpers.CreateDbContext();
  }

  [Fact]
  public async Task Database_Should_Connect()
  {
    var canConnect = await _db.Database.CanConnectAsync(); ;
    Assert.True(canConnect);
  }

  [Fact]
  public async Task Answers_Should_Match()
  {
    var contentfulAnswers = _contentfulContent.GetEntriesForContentType("answer")
                                              .ToList();

    Assert.NotEmpty(contentfulAnswers);

    var databaseAnswers = await _db.Answers.ToListAsync();

    Assert.NotEmpty(databaseAnswers);

    foreach (var contentfulAnswer in contentfulAnswers)
    {
      var databaseAnswer = databaseAnswers.FirstOrDefault(answer => answer.Id == contentfulAnswer.GetEntryId());
      Assert.NotNull(databaseAnswer);

      Assert.Equal(contentfulAnswer["text"]?.GetValue<string>(), databaseAnswer.Text);
      Assert.Equal(contentfulAnswer["maturity"]?.GetValue<string>(), databaseAnswer.Maturity);
      Assert.Equal(contentfulAnswer["nextQuestion"]?["sys"]?["id"]?.GetValue<string>(), databaseAnswer.NextQuestionId);
    }
  }

  [Fact]
  public async Task Questions_Should_Match()
  {
    var contentfulQuestions = _contentfulContent.GetEntriesForContentType("question")
                                              .ToList();

    Assert.NotEmpty(contentfulQuestions);

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

    Assert.NotEmpty(databaseQuestions);

    foreach (var contentfulQuestion in contentfulQuestions)
    {
      var databaseQuestion = databaseQuestions.FirstOrDefault(question => question.Id == contentfulQuestion.GetEntryId());
      Assert.NotNull(databaseQuestion);

      Assert.Equal(contentfulQuestion["text"]?.GetValue<string>(), databaseQuestion.Text);
      Assert.Equal(contentfulQuestion["helpText"]?.GetValue<string>(), databaseQuestion.HelpText);
      Assert.Equal(contentfulQuestion["slug"]?.GetValue<string>(), databaseQuestion.Slug);

      var questionAnswers = contentfulQuestion["answers"]?.AsArray() ?? throw new JsonException("No answers for question");

      foreach (var questionAnswer in questionAnswers)
      {
        var questionAnswerId = questionAnswer!["sys"]?["id"]?.GetValue<string>() ?? throw new JsonException($"Couldn't find Id in {questionAnswer}");
        var exists = databaseQuestion.Answers.Exists(answer => answer.Id == questionAnswerId);

        Assert.True(exists);
      }
    }
  }

  [Fact]
  public async Task Buttons_Should_Match()
  {
    var contentfulButtons = _contentfulContent.GetEntriesForContentType("button")
                                              .ToList();

    Assert.NotEmpty(contentfulButtons);

    var databaseButtons = await _db.Buttons.ToListAsync();

    Assert.NotEmpty(databaseButtons);

    foreach (var contentfulButton in contentfulButtons)
    {
      var databaseButton = databaseButtons.FirstOrDefault(button => button.Id == contentfulButton.GetEntryId());
      Assert.NotNull(databaseButton);

      Assert.Equal(contentfulButton["value"]?.GetValue<string>(), databaseButton.Value);
      Assert.Equal(contentfulButton["isStartButton"]?.GetValue<bool>(), databaseButton.IsStartButton);
    }
  }

  [Fact]
  public async Task ButtonWithLinks_Should_Match()
  {
    var contentfulButtons = _contentfulContent.GetEntriesForContentType("buttonWithLink")
                                              .ToList();

    Assert.NotEmpty(contentfulButtons);

    var databaseButtons = await _db.ButtonWithLinks.ToListAsync();

    Assert.NotEmpty(databaseButtons);

    foreach (var contentfulButton in contentfulButtons)
    {
      var databaseButton = databaseButtons.FirstOrDefault(button => button.Id == contentfulButton.GetEntryId());
      Assert.NotNull(databaseButton);

      Assert.Equal(contentfulButton["href"]?.GetValue<string>(), databaseButton.Href);

      var buttonId = contentfulButton["button"]?["sys"]?["id"]?.GetValue<string>();
      Assert.Equal(buttonId, databaseButton.ButtonId);
    }
  }

}