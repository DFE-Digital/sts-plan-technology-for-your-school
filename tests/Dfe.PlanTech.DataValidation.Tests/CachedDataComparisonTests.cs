using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

  [Fact]
  public async Task ButtonWithEntryReferences_Should_Match()
  {
    var contentfulButtons = _contentfulContent.GetEntriesForContentType("buttonWithEntryReference")
                                              .ToList();

    Assert.NotEmpty(contentfulButtons);

    var databaseButtons = await _db.ButtonWithEntryReferences.ToListAsync();

    Assert.NotEmpty(databaseButtons);

    foreach (var contentfulButton in contentfulButtons)
    {
      var databaseButton = databaseButtons.FirstOrDefault(button => button.Id == contentfulButton.GetEntryId());
      Assert.NotNull(databaseButton);

      var buttonId = contentfulButton["button"]?["sys"]?["id"]?.GetValue<string>();
      Assert.Equal(buttonId, databaseButton.ButtonId);

      var entryReference = contentfulButton["linkToEntry"]?["sys"]?["id"]?.GetValue<string>();
      Assert.Equal(entryReference, databaseButton.LinkToEntryId);
    }
  }

  [Fact]
  public async Task ComponentDropDowns_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<ComponentDropDownDbEntity>();
    await comparator.LoadDbEntities(_db.ComponentDropDowns);
    await _db.RichTextContents.Include(content => content.Marks)
                              .Include(content => content.Data)
                              .ToListAsync();

    comparator.LoadContentfulEntities(_contentfulContent, "componentDropDown");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
                              {
                                Assert.Equal(contentfulEntry["title"]?.GetValue<string>(), dbEntry.Title);

                                var parentContent = contentfulEntry["content"] ?? throw new Exception("Parent content not found");

                                CompareRichTextContent(dbEntry.Content, parentContent);
                              });
  }

  // Compare categories between database and contentful entities.
  [Fact]
  public async Task Categories_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<CategoryDbEntity>();
    await comparator.LoadDbEntities(_db.Categories.Select(category => new CategoryDbEntity()
    {
      InternalName = category.InternalName,
      Id = category.Id,
      HeaderId = category.HeaderId,
      Sections = category.Sections.Select(section => new SectionDbEntity()
      {
        Id = section.Id
      }).ToList()
    }));

    comparator.LoadContentfulEntities(_contentfulContent, "category");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
    {
      Assert.Equal(contentfulEntry["internalName"]?.GetValue<string>(), dbEntry.InternalName);
      Assert.Equal(contentfulEntry["header"]?["sys"]?["id"]?.GetValue<string>(), dbEntry.HeaderId);

      comparator.ValidateArrayReferences(contentfulEntry, "sections", dbEntry, (entry) => entry.Sections);
    });
  }

  [Fact]
  public async Task Headers_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<HeaderDbEntity>();
    await comparator.LoadDbEntities(_db.Headers);

    comparator.LoadContentfulEntities(_contentfulContent, "header");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
    {
      Assert.Equal(contentfulEntry["text"]?.GetValue<string>(), dbEntry.Text);

      comparator.ValidateEnumValue(contentfulEntry, "tag", dbEntry, dbEntry.Tag);
      comparator.ValidateEnumValue(contentfulEntry, "size", dbEntry, dbEntry.Size);
    });
  }

  [Fact]
  public async Task InsetTexts_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<InsetTextDbEntity>();
    await comparator.LoadDbEntities(_db.InsetTexts);

    comparator.LoadContentfulEntities(_contentfulContent, "insetText");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
    {
      Assert.Equal(contentfulEntry["text"]?.GetValue<string>(), dbEntry.Text);
    });
  }

  [Fact]
  public async Task Pages_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<PageDbEntity>();
    await comparator.LoadDbEntities(_db.Pages);

    comparator.LoadContentfulEntities(_contentfulContent, "page");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
    {
      Assert.Equal(contentfulEntry["internalName"]?.GetValue<string>(), dbEntry.InternalName);
      Assert.Equal(contentfulEntry["slug"]?.GetValue<string>(), dbEntry.Slug);
      Assert.Equal(contentfulEntry["displayHomeButton"]?.GetValue<bool>() ?? false, dbEntry.DisplayHomeButton);
      Assert.Equal(contentfulEntry["displayBackButton"]?.GetValue<bool>() ?? false, dbEntry.DisplayBackButton);
      Assert.Equal(contentfulEntry["displayTopicTitle"]?.GetValue<bool>() ?? false, dbEntry.DisplayTopicTitle);
      Assert.Equal(contentfulEntry["displayOrganisationName"]?.GetValue<bool>() ?? false, dbEntry.DisplayOrganisationName);
      Assert.Equal(contentfulEntry["requiresAuthorisation"]?.GetValue<bool>() ?? false, dbEntry.RequiresAuthorisation);
      Assert.Equal(contentfulEntry["title"]?["sys"]?["id"]?.GetValue<string>(), dbEntry.TitleId);

      _db.Entry(dbEntry).Collection(page => page.Content).Load();
      comparator.ValidateArrayReferences(contentfulEntry, "content", dbEntry, (entry) => entry.Content);

      _db.Entry(dbEntry).Collection(page => page.BeforeTitleContent).Load();
      comparator.ValidateArrayReferences(contentfulEntry, "beforeTitleContent", dbEntry, (entry) => entry.BeforeTitleContent);

    });
  }

  [Fact]
  public async Task NavigationLinks_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<NavigationLinkDbEntity>();
    await comparator.LoadDbEntities(_db.NavigationLink);

    comparator.LoadContentfulEntities(_contentfulContent, "navigationLink");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
    {
      Assert.Equal(contentfulEntry["displayText"]?.GetValue<string>(), dbEntry.DisplayText);
      Assert.Equal(contentfulEntry["href"]?.GetValue<string>(), dbEntry.Href);
      Assert.Equal(contentfulEntry["openInNewTab"]?.GetValue<bool>() ?? false, dbEntry.OpenInNewTab);
    });
  }

  [Fact]
  public async Task RecommendationPages_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<RecommendationPageDbEntity>();
    await comparator.LoadDbEntities(_db.RecommendationPages);

    comparator.LoadContentfulEntities(_contentfulContent, "recommendationPage");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
                              {
                                Assert.Equal(contentfulEntry["internalName"]?.GetValue<string>(), dbEntry.InternalName);
                                Assert.Equal(contentfulEntry["displayName"]?.GetValue<string>(), dbEntry.DisplayName);
                                Assert.Equal(contentfulEntry["page"]?["sys"]?["id"]?.GetValue<string>(), dbEntry.PageId);

                                comparator.ValidateEnumValue(contentfulEntry, "maturity", dbEntry, dbEntry.Maturity);
                              });
  }

  // Test to verify that the sections match between the database and the Contentful API.
  [Fact]
  public async Task Sections_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<SectionDbEntity>();
    await comparator.LoadDbEntities(_db.Sections.Select(section => new SectionDbEntity()
    {
      Id = section.Id,
      InterstitialPageId = section.InterstitialPageId,
      Questions = section.Questions.Select(question => new QuestionDbEntity()
      {
        Id = question.Id,
      }).ToList(),
      Recommendations = section.Recommendations.Select(recommendation => new RecommendationPageDbEntity()
      {
        Id = recommendation.Id
      }).ToList(),
      Name = section.Name
    }));

    comparator.LoadContentfulEntities(_contentfulContent, "section");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
                              {
                                Assert.Equal(contentfulEntry["name"]?.GetValue<string>(), dbEntry.Name);
                                Assert.Equal(contentfulEntry["interstitialPage"]?["sys"]?["id"]?.GetValue<string>(), dbEntry.InterstitialPageId);
                                comparator.ValidateArrayReferences(contentfulEntry, "questions", dbEntry, (entry) => entry.Questions);
                                comparator.ValidateArrayReferences(contentfulEntry, "recommendations", dbEntry, (entry) => entry.Recommendations);
                              });
  }

  [Fact]
  public async Task TextBodies_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<TextBodyDbEntity>();
    await comparator.LoadDbEntities(_db.TextBodies);
    await _db.RichTextContents.Include(content => content.Marks)
                              .Include(content => content.Data)
                              .ToListAsync();


    comparator.LoadContentfulEntities(_contentfulContent, "textBody");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
                              {
                                var richTextContentParent = contentfulEntry["richText"] ?? throw new Exception("Parent content not found");
                                CompareRichTextContent(dbEntry.RichText, richTextContentParent);
                              });
  }


  [Fact]
  public async Task Titles_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<TitleDbEntity>();
    await comparator.LoadDbEntities(_db.Titles);

    comparator.LoadContentfulEntities(_contentfulContent, "title");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
                              {
                                Assert.Equal(contentfulEntry["text"]?.GetValue<string>(), dbEntry.Text);
                              });
  }


  [Fact]
  public async Task WarningComponents_Should_Match()
  {
    var comparator = new ContentfulDatabaseComparator<WarningComponentDbEntity>();
    await comparator.LoadDbEntities(_db.Warnings.Select(warning => new WarningComponentDbEntity()
    {
      Id = warning.Id,
      TextId = warning.TextId
    }));

    await _db.RichTextContents.Include(content => content.Marks)
                          .Include(content => content.Data)
                          .ToListAsync();

    comparator.LoadContentfulEntities(_contentfulContent, "warningComponent");

    comparator.CompareContent((contentfulEntry, dbEntry) =>
                              {
                                Assert.Equal(contentfulEntry["text"]?["sys"]?["id"]?.GetValue<string>(), dbEntry.TextId);
                              });
  }

  private static void CompareRichTextContent(RichTextContentDbEntity dbEntry, JsonNode content)
  {
    Assert.Equal(content?["nodeType"]?.GetValue<string>(), dbEntry.NodeType);

    var value = content?["value"]?.GetValue<string>() ?? "";
    Assert.Equal(value, dbEntry.Value);

    var data = content?["data"];
    Assert.Equal(data?["uri"]?.GetValue<string>(), dbEntry.Data?.Uri);

    var children = content?["content"]?.AsArray();

    if (children != null)
    {
      Assert.Equal(children.Count, dbEntry.Content.Count);

      for (var x = 0; x < children.Count; x++)
      {
        CompareRichTextContent(dbEntry.Content[x], children[x]!);
      }
    }
  }

}