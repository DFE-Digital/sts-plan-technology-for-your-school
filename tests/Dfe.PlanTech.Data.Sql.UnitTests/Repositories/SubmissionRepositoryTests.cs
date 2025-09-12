using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Repositories;

public class SubmissionRepositoryTests
{
    private static PlanTechDbContext BuildPlanTechDbContext(string name)
    {
        var options = new DbContextOptionsBuilder<PlanTechDbContext>()
            .UseInMemoryDatabase(name)
            .EnableSensitiveDataLogging()
            .Options;
        var ctx = new PlanTechDbContext(options);
        return ctx;
    }

    private static QuestionEntity BuildQuestion(int id)
    {
        return new QuestionEntity
        {
            Id = id,
            QuestionText = $"Question {id}",
            ContentfulRef = $"Q{id}",
            DateCreated = DateTime.UtcNow.AddMinutes(-20)
        };
    }
    private static AnswerEntity BuildAnswer(int id)
    {
        return new AnswerEntity
        {
            Id = id,
            AnswerText = $"Answer {id}",
            ContentfulRef = $"A{id}",
            DateCreated = DateTime.UtcNow.AddMinutes(-20)
        };
    }

    [Fact]
    public async Task CloneSubmission_Throws_When_Null()
    {
        using var db = BuildPlanTechDbContext(nameof(CloneSubmission_Throws_When_Null));
        var repo = new SubmissionRepository(db);

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CloneSubmission(null));
    }

    [Fact]
    public async Task CloneSubmission_Copies_Fields_Resets_State_And_Persists()
    {
        using var db = BuildPlanTechDbContext(nameof(CloneSubmission_Copies_Fields_Resets_State_And_Persists));
        var repo = new SubmissionRepository(db);

        var q = BuildQuestion(1);
        var a = BuildAnswer(2);

        db.Questions.Add(q);
        db.Answers.Add(a);

        var existing = new SubmissionEntity
        {
            SectionId = "SEC",
            SectionName = "Section Name",
            EstablishmentId = 123,
            Completed = true,
            Maturity = "developing",
            DateCreated = DateTime.UtcNow.AddDays(-1),
            Status = SubmissionStatus.CompleteReviewed.ToString(),
            Responses = new List<ResponseEntity>
            {
                new ResponseEntity
                {
                    QuestionId = 1,
                    AnswerId = 2,
                    UserId = 999,
                    Maturity = "developing",
                    DateCreated = DateTime.UtcNow.AddMinutes(-10)
                }
            }
        };

        var before = DateTime.UtcNow.AddSeconds(-2);
        var clone = await repo.CloneSubmission(existing);
        var after = DateTime.UtcNow.AddSeconds(2);

        Assert.NotEqual(0, clone.Id);
        Assert.Equal("SEC", clone.SectionId);
        Assert.Equal("Section Name", clone.SectionName);
        Assert.Equal(123, clone.EstablishmentId);
        Assert.False(clone.Completed);
        Assert.Equal("developing", clone.Maturity);
        Assert.Equal(SubmissionStatus.InProgress.ToString(), clone.Status);
        Assert.InRange(clone.DateCreated, before, after);

        var r = Assert.Single(clone.Responses);
        Assert.Equal(1, r.QuestionId);
        Assert.Equal(2, r.AnswerId);
        Assert.Equal(999, r.UserId);
        Assert.Equal("developing", r.Maturity);
        Assert.Same(q, r.Question); // same navs copied
        Assert.Same(a, r.Answer);
        Assert.InRange(r.DateCreated, before, after);

        // persisted
        Assert.Equal(1, await db.Submissions.CountAsync());
        Assert.Equal(1, await db.Responses.CountAsync());
    }

    [Fact]
    public async Task GetLatestSubmissionAndResponses_Returns_Null_When_None()
    {
        using var db = BuildPlanTechDbContext(nameof(GetLatestSubmissionAndResponses_Returns_Null_When_None));
        var repo = new SubmissionRepository(db);

        var result = await repo.GetLatestSubmissionAndResponsesAsync(1, "SEC", null);
        Assert.Null(result);
    }

    [Fact]
    public async Task SetLatestSubmissionViewed_No_Submission_No_Throw()
    {
        using var db = BuildPlanTechDbContext(nameof(SetLatestSubmissionViewed_No_Submission_No_Throw));
        var repo = new SubmissionRepository(db);

        await repo.SetLatestSubmissionViewedAsync(99, "NA");
        // nothing to assert beyond “no throw” and SaveChanges called; InMemory SaveChanges is cheap.
        Assert.Equal(0, await db.Submissions.CountAsync());
    }

    [Fact]
    public async Task SetSubmissionReviewed_Throws_When_Submission_Not_Found()
    {
        using var db = BuildPlanTechDbContext(nameof(SetSubmissionReviewed_Throws_When_Submission_Not_Found));
        var repo = new SubmissionRepository(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(999));
    }

    [Fact]
    public async Task SetSubmissionInaccessible_ById_Throws_When_NotFound()
    {
        using var db = BuildPlanTechDbContext(nameof(SetSubmissionInaccessible_ById_Throws_When_NotFound));
        var repo = new SubmissionRepository(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.SetSubmissionInaccessibleAsync(12345));
    }

    [Fact]
    public async Task SetSubmissionInaccessible_ByScope_Throws_When_NotFound()
    {
        using var db = BuildPlanTechDbContext(nameof(SetSubmissionInaccessible_ByScope_Throws_When_NotFound));
        var repo = new SubmissionRepository(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.SetSubmissionInaccessibleAsync(establishmentId: 99, sectionId: "NA"));
    }
}
