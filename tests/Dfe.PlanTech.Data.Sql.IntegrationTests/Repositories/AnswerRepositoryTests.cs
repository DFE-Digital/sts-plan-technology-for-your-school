using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class AnswerRepositoryTests : DatabaseIntegrationTestBase
{
    private IAnswerRepository _answerRepository = null!;

    public AnswerRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _answerRepository = new AnswerRepository(DbContext);
    }

    [Fact]
    public async Task GetOrCreateAnswerAsync_WhenAnswerDoesNotExist_ThenAdds()
    {
        var contentfulRef = "Q001";
        var AnswerText = "Answer 001";

        var initialSubmissionCount = await DbContext.Answers.CountAsync(
            TestContext.Current.CancellationToken
        );

        await _answerRepository.GetOrCreateAnswerIdAsync(contentfulRef, AnswerText);

        var finalSubmissionCount = await DbContext.Answers.CountAsync(
            TestContext.Current.CancellationToken
        );

        Assert.Equal(finalSubmissionCount, initialSubmissionCount + 1);
    }

    [Fact]
    public async Task GetOrCreateAnswerAsync_WhenAnswerExists_ThenReturnsExistingId()
    {
        var contentfulRef = "Q001";
        var AnswerText = "Answer 001";

        var answer = new AnswerEntity { ContentfulRef = contentfulRef, AnswerText = AnswerText };

        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var initialSubmissionCount = await DbContext.Answers.CountAsync(
            TestContext.Current.CancellationToken
        );

        var answerId = await _answerRepository.GetOrCreateAnswerIdAsync(contentfulRef, AnswerText);

        var finalSubmissionCount = await DbContext.Answers.CountAsync(
            TestContext.Current.CancellationToken
        );

        Assert.Equal(finalSubmissionCount, initialSubmissionCount);
        Assert.Equal(answer.Id, answerId);
    }
}
