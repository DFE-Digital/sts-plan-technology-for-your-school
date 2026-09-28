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

    //[Fact]
    //public async Task GetResponsesByContentfulReferencesAsync_WhenNoMatchingReferences_ThenReturnsEmpty()
    //{
    //    // Arrange - Create a response with specific ContentfulRef and search for non-matching references
    //    var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
    //    DbContext.Questions.Add(question);
    //    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var response = new ResponseEntity
    //    {
    //        ResponseText = "Test Response",
    //        ContentfulRef = "rec-001",
    //        QuestionId = question.Id,
    //    };

    //    DbContext.Responses.Add(response);
    //    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var nonMatchingReferences = new[] { "rec-999", "rec-888" };

    //    // Act
    //    var result = await _repository.GetResponsesByContentfulReferencesAsync(
    //        nonMatchingReferences
    //    );

    //    // Assert
    //    Assert.Empty(result);
    //}

    //[Fact]
    //public async Task GetResponsesByContentfulReferencesAsync_WhenEmptyReferences_ThenReturnsEmpty()
    //{
    //    // Arrange - Create a response but search with empty reference array
    //    var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
    //    DbContext.Questions.Add(question);
    //    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var response = new ResponseEntity
    //    {
    //        ResponseText = "Test Response",
    //        ContentfulRef = "rec-001",
    //        QuestionId = question.Id,
    //    };

    //    DbContext.Responses.Add(response);
    //    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var emptyReferences = new string[0];

    //    // Act
    //    var result = await _repository.GetResponsesByContentfulReferencesAsync(
    //        emptyReferences
    //    );

    //    // Assert
    //    Assert.Empty(result);
    //}

    //[Fact]
    //public async Task GetResponsesByContentfulReferencesAsync_WhenDuplicateReferences_ThenReturnsDistinctResults()
    //{
    //    // Arrange - Create one response and search with duplicate references to test deduplication
    //    var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
    //    DbContext.Questions.Add(question);
    //    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var response = new ResponseEntity
    //    {
    //        ResponseText = "Test Response",
    //        ContentfulRef = "rec-001",
    //        QuestionId = question.Id,
    //    };

    //    DbContext.Responses.Add(response);
    //    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var duplicateReferences = new[] { "rec-001", "rec-001", "rec-001" };

    //    // Act
    //    var result = await _repository.GetResponsesByContentfulReferencesAsync(
    //        duplicateReferences
    //    );

    //    // Assert
    //    var responses = result.ToList();
    //    Assert.Single(responses);
    //    Assert.Equal("rec-001", responses.First().ContentfulRef);
    //}

    //[Fact]
    //public async Task GetResponsesByContentfulReferencesAsync_WhenIncludesArchivedResponses_ThenReturnsAllMatches()
    //{
    //    // Arrange - Create both active and archived responses to test that both are returned
    //    var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
    //    DbContext.Questions.Add(question);
    //    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var activeResponse = new ResponseEntity
    //    {
    //        ResponseText = "Active Response",
    //        ContentfulRef = "rec-active",
    //        QuestionId = question.Id,
    //        Archived = false,
    //    };

    //    var archivedResponse = new ResponseEntity
    //    {
    //        ResponseText = "Archived Response",
    //        ContentfulRef = "rec-archived",
    //        QuestionId = question.Id,
    //        Archived = true,
    //    };

    //    DbContext.Responses.AddRange(activeResponse, archivedResponse);
    //    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    //    var references = new[] { "rec-active", "rec-archived" };

    //    // Act
    //    var result = await _repository.GetResponsesByContentfulReferencesAsync(references);

    //    // Assert
    //    var responses = result.ToList();
    //    Assert.Equal(2, responses.Count);
    //    Assert.Contains(responses, r => r.ContentfulRef == "rec-active" && !r.Archived);
    //    Assert.Contains(responses, r => r.ContentfulRef == "rec-archived" && r.Archived);
    //}

    [Fact]
    public async Task SubmitResponseAsync_WhenSubmissionAlreadyExists_ThenPersistsEachResponse()
    {
        // Arrange - Answering a second question against an existing submission must persist on its
        // own, rather than relying on a later SaveChanges elsewhere in the request to flush it.
        var user = EntityBuilders.BuildUser(102);
        await DbContext.Users.AddAsync(user, TestContext.Current.CancellationToken);
        await SetIdentityInsert("user", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("user", "OFF");

        var establishment = EntityBuilders.BuildEstablishment(202);
        await DbContext.Establishments.AddAsync(
            establishment,
            TestContext.Current.CancellationToken
        );
        await SetIdentityInsert("establishment", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("establishment", "OFF");

        var submission = EntityBuilders.BuildSubmission(302, establishment, "SEC2");
        await DbContext.Submissions.AddAsync(submission, TestContext.Current.CancellationToken);
        await SetIdentityInsert("submission", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("submission", "OFF");

        var firstQuestion = EntityBuilders.BuildQuestion(402);
        var secondQuestion = EntityBuilders.BuildQuestion(403);
        await DbContext.Questions.AddRangeAsync(
            [firstQuestion, secondQuestion],
            TestContext.Current.CancellationToken
        );
        await SetIdentityInsert("question", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("question", "OFF");

        var firstAnswer = EntityBuilders.BuildAnswer(502);
        var secondAnswer = EntityBuilders.BuildAnswer(503);
        await DbContext.Answers.AddRangeAsync(
            [firstAnswer, secondAnswer],
            TestContext.Current.CancellationToken
        );
        await SetIdentityInsert("answer", "ON");
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SetIdentityInsert("answer", "OFF");

        // Act
        var firstResponseId = await _repository.SubmitResponseAsync(
            user.Id,
            establishment.Id,
            submission.Id,
            firstQuestion.Id,
            firstAnswer.Id
        );

        var secondResponseId = await _repository.SubmitResponseAsync(
            user.Id,
            establishment.Id,
            submission.Id,
            secondQuestion.Id,
            secondAnswer.Id
        );

        // Assert
        Assert.NotEqual(0, firstResponseId);
        Assert.NotEqual(0, secondResponseId);
        Assert.NotEqual(firstResponseId, secondResponseId);

        var responses = await DbContext
            .Responses.Where(r => r.SubmissionId == submission.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, responses.Count);
        Assert.Contains(responses, r => r.QuestionId == firstQuestion.Id);
        Assert.Contains(responses, r => r.QuestionId == secondQuestion.Id);
    }

    private class TestUserActionIdAccessor(Guid userActionId) : IUserActionIdProvider
    {
        public Guid GetUserActionId() => userActionId;
    }
}
