using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class QuestionRepositoryTests : DatabaseIntegrationTestBase
{
    private IQuestionRepository _questionRepository = null!;

    public QuestionRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _questionRepository = new QuestionRepository(DbContext);
    }

    [Fact]
    public async Task GetQuestionsForSection_ReturnsMatchingQuestions_WhenContentfulRefsMatch()
    {
        // Arrange
        var question1 = new QuestionEntity { ContentfulRef = "ref-101" };
        var question2 = new QuestionEntity { ContentfulRef = "ref-102" };

        DbContext.Questions.AddRange(question1, question2);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sectionQuestions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "ref-101" } },
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "ref-102" } },
        };
        var section = new QuestionnaireSectionEntry { Questions = sectionQuestions };

        // Act
        var result = await _questionRepository.GetQuestionsForSection(section);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, q => q.ContentfulRef == "ref-101");
        Assert.Contains(result, q => q.ContentfulRef == "ref-102");
    }

    [Fact]
    public async Task GetQuestionsForSection_ReturnsEmptyList_WhenNoMatchingQuestions()
    {
        // Arrange
        var question = new QuestionEntity { ContentfulRef = "ref-201" };

        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var section = new QuestionnaireSectionEntry
        {
            Questions =
            [
                new QuestionnaireQuestionEntry
                {
                    Sys = new SystemDetails { Id = "non-existent-ref" },
                },
            ],
        };

        // Act
        var result = await _questionRepository.GetQuestionsForSection(section);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetQuestionsForSection_IgnoresNullSysIds()
    {
        // Arrange
        var question = new QuestionEntity { ContentfulRef = "ref-301" };

        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var section = new QuestionnaireSectionEntry
        {
            Questions = new List<QuestionnaireQuestionEntry>
            {
                new QuestionnaireQuestionEntry { Sys = null },
                new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = null! } },
                new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "ref-301" } },
            },
        };

        // Act
        var result = await _questionRepository.GetQuestionsForSection(section);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ref-301", result.First().ContentfulRef);
    }

    [Fact]
    public async Task GetOrCreateQuestionAsync_WhenQuestionDoesNotExist_ThenAdds()
    {
        var contentfulRef = "Q001";
        var questionText = "Question 001";

        var initialSubmissionCount = await DbContext.Questions.CountAsync(
            TestContext.Current.CancellationToken
        );

        await _questionRepository.GetOrCreateQuestionIdAsync(contentfulRef, questionText);

        var finalSubmissionCount = await DbContext.Questions.CountAsync(
            TestContext.Current.CancellationToken
        );

        Assert.Equal(finalSubmissionCount, initialSubmissionCount + 1);
    }

    [Fact]
    public async Task GetOrCreateQuestionAsync_WhenQuestionExists_ThenReturnsExistingId()
    {
        var contentfulRef = "Q001";
        var questionText = "Question 001";

        var question = new QuestionEntity
        {
            ContentfulRef = contentfulRef,
            QuestionText = questionText,
        };

        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var initialSubmissionCount = await DbContext.Questions.CountAsync(
            TestContext.Current.CancellationToken
        );

        var questionId = await _questionRepository.GetOrCreateQuestionIdAsync(
            contentfulRef,
            questionText
        );

        var finalSubmissionCount = await DbContext.Questions.CountAsync(
            TestContext.Current.CancellationToken
        );

        Assert.Equal(finalSubmissionCount, initialSubmissionCount);
        Assert.Equal(question.Id, questionId);
    }
}
