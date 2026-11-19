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
