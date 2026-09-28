using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Repositories;
using Dfe.PlanTech.UnitTests.Shared.Builders;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class ResponseRepositoryTests : DatabaseIntegrationTestBase
{
    private ResponseRepository _repository = null!;
    private readonly Guid _userActionId = Guid.NewGuid();

    public ResponseRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new ResponseRepository(
            DbContext,
            new TestUserActionIdAccessor(_userActionId)
        );
    }

    [Fact]
    public async Task SubmitResponseAsync_WritesResponse()
    {
        // Arrange - Create multiple responses with different ContentfulRef values and search for specific ones
        var questionEntry = EntryBuilders.BuildQuestion("Q1");
        var recommendation = EntryBuilders.BuildRecommendationChunk("REC1", questionEntry.Id);
        var section = EntryBuilders.BuildSection(recommendation, [questionEntry]);

        var user = EntityBuilders.BuildUser(101);
        await DbContext.Users.AddAsync(user, TestContext.Current.CancellationToken);
        await SetIdentityInsert("user", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("user", "OFF");

        var establishment = EntityBuilders.BuildEstablishment(201);
        await DbContext.Establishments.AddAsync(
            establishment,
            TestContext.Current.CancellationToken
        );
        await SetIdentityInsert("establishment", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("establishment", "OFF");

        var submission = EntityBuilders.BuildSubmission(301, establishment, section.Id);
        await DbContext.Submissions.AddAsync(submission, TestContext.Current.CancellationToken);
        await SetIdentityInsert("submission", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("submission", "OFF");

        var question = EntityBuilders.BuildQuestion(401);
        await DbContext.Questions.AddAsync(question, TestContext.Current.CancellationToken);
        await SetIdentityInsert("question", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("question", "OFF");

        var answer = EntityBuilders.BuildAnswer(501);
        await DbContext.Answers.AddAsync(answer, TestContext.Current.CancellationToken);
        await SetIdentityInsert("answer", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("answer", "OFF");

        // Act
        var newResponseId = await _repository.SubmitResponseAsync(
            user.Id,
            establishment.Id,
            submission.Id,
            question.Id,
            answer.Id
        );

        // Assert
        var responses = await DbContext
            .Responses.Where(r => r.Id == newResponseId)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Single(responses);
        Assert.Contains(responses, r => r.SubmissionId == submission.Id);
        Assert.Contains(responses, r => r.QuestionId == question.Id);
        Assert.Contains(responses, r => r.AnswerId == answer.Id);
        Assert.Contains(responses, r => r.UserActionId == _userActionId);
    }

    private class TestUserActionIdAccessor(Guid userActionId) : IUserActionIdProvider
    {
        public Guid GetUserActionId() => userActionId;
    }
}
