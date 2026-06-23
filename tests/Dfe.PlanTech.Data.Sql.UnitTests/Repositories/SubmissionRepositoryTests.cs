using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

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

    private static EstablishmentEntity BuildEstablishmentEntity(int id)
    {
        return new EstablishmentEntity
        {
            Id = id,
            DateCreated = DateTime.UtcNow.AddDays(-30),
            DateLastUpdated = DateTime.UtcNow.AddDays(-30),
            EstablishmentRef = "EST" + id,
            EstablishmentType = "School",
        };
    }

    private static IUserActionIdProvider BuildUserActionIdAccessor()
    {
        var accessor = Substitute.For<IUserActionIdProvider>();
        accessor.GetUserActionId().Returns(Guid.NewGuid());
        return accessor;
    }

    private static QuestionEntity BuildQuestion(int id)
    {
        return new QuestionEntity
        {
            Id = id,
            QuestionText = $"Question {id}",
            ContentfulRef = $"Q{id}",
            DateCreated = DateTime.UtcNow.AddMinutes(-20),
        };
    }

    private static AnswerEntity BuildAnswer(int id)
    {
        return new AnswerEntity
        {
            Id = id,
            AnswerText = $"Answer {id}",
            ContentfulRef = $"A{id}",
            DateCreated = DateTime.UtcNow.AddMinutes(-20),
        };
    }

    [Fact]
    public async Task CloneSubmission_Throws_When_Null()
    {
        using var db = BuildPlanTechDbContext(nameof(CloneSubmission_Throws_When_Null));
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CloneSubmission(null));
    }

    [Fact]
    public async Task CloneSubmission_Copies_Fields_Resets_State_And_Persists()
    {
        using var db = BuildPlanTechDbContext(
            nameof(CloneSubmission_Copies_Fields_Resets_State_And_Persists)
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        var q = BuildQuestion(1);
        var a = BuildAnswer(2);

        db.Questions.Add(q);
        db.Answers.Add(a);

        var existing = new SubmissionEntity
        {
            SectionId = "SEC",
            SectionName = "Section Name",
            EstablishmentId = 123,
            DateCreated = DateTime.UtcNow.AddDays(-1),
            Status = SubmissionStatus.CompleteReviewed,
            Responses = new List<ResponseEntity>
            {
                new ResponseEntity
                {
                    QuestionId = 1,
                    AnswerId = 2,
                    UserId = 999,
                    DateCreated = DateTime.UtcNow.AddMinutes(-10),
                },
            },
        };

        var before = DateTime.UtcNow.AddSeconds(-2);
        var clone = await repo.CloneSubmission(existing);
        var after = DateTime.UtcNow.AddSeconds(2);

        Assert.NotEqual(0, clone.Id);
        Assert.Equal("SEC", clone.SectionId);
        Assert.Equal("Section Name", clone.SectionName);
        Assert.Equal(123, clone.EstablishmentId);
        Assert.Equal(SubmissionStatus.InProgress, clone.Status);
        Assert.InRange(clone.DateCreated, before, after);

        Assert.NotNull(clone.CreatedUserActionId);
        Assert.NotNull(clone.LastUpdatedUserActionId);

        var r = Assert.Single(clone.Responses);
        Assert.Equal(1, r.QuestionId);
        Assert.Equal(2, r.AnswerId);
        Assert.Equal(999, r.UserId);
        Assert.Same(q, r.Question);
        Assert.Same(a, r.Answer);
        Assert.InRange(r.DateCreated, before, after);

        Assert.Equal(1, await db.Submissions.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await db.Responses.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLatestSubmissionAndResponses_Returns_Null_When_None()
    {
        using var db = BuildPlanTechDbContext(
            nameof(GetLatestSubmissionAndResponses_Returns_Null_When_None)
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        var result = await repo.GetLatestSubmissionAndResponsesAsync(
            1,
            "SEC",
            (SubmissionStatus?)null
        );
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestSubmissionAndResponses_MultipleStatuses_Returns_Null_When_None()
    {
        using var db = BuildPlanTechDbContext(
            nameof(GetLatestSubmissionAndResponses_MultipleStatuses_Returns_Null_When_None)
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        var result = await repo.GetLatestSubmissionAndResponsesAsync(
            1,
            "SEC",
            [SubmissionStatus.CompleteReviewed]
        );
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestSubmissionAndResponses_MultipleStatuses_Throws_When_Empty_Status_Enumerable()
    {
        using var db = BuildPlanTechDbContext(
            nameof(
                GetLatestSubmissionAndResponses_MultipleStatuses_Throws_When_Empty_Status_Enumerable
            )
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        var result = await Assert.ThrowsAsync<ArgumentException>(() =>
            repo.GetLatestSubmissionAndResponsesAsync(1, "SEC", [])
        );

        Assert.Contains("At least one submission status must be provided", result.Message);
    }

    [Fact]
    public async Task GetLatestSubmissionAndResponses_MultipleStatuses_Returns_Latest_Of_Included_Statuses()
    {
        using var db = BuildPlanTechDbContext(
            nameof(
                GetLatestSubmissionAndResponses_MultipleStatuses_Returns_Latest_Of_Included_Statuses
            )
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        db.Establishments.Add(BuildEstablishmentEntity(1));

        db.Submissions.AddRange(
            new SubmissionEntity
            {
                Id = 1,
                EstablishmentId = 1,
                SectionId = "SEC",
                SectionName = "Section name",
                Status = SubmissionStatus.InProgress,
                DateCreated = new DateTime(2024, 1, 1),
                DateCompleted = new DateTime(2024, 1, 1),
                DateLastUpdated = new DateTime(2024, 1, 1),
            },
            new SubmissionEntity
            {
                Id = 2,
                EstablishmentId = 1,
                SectionId = "SEC",
                SectionName = "Section name",
                Status = SubmissionStatus.Inaccessible,
                DateCreated = new DateTime(2025, 1, 1),
                DateLastUpdated = new DateTime(2025, 1, 1),
            },
            new SubmissionEntity
            {
                Id = 3,
                EstablishmentId = 1,
                SectionId = "SEC",
                SectionName = "Section name",
                Status = SubmissionStatus.CompleteReviewed,
                DateCreated = new DateTime(2026, 1, 1),
                DateCompleted = new DateTime(2026, 1, 1),
                DateLastUpdated = new DateTime(2026, 1, 1),
            }
        );

        db.SaveChanges();

        var result = await repo.GetLatestSubmissionAndResponsesAsync(
            1,
            "SEC",
            [SubmissionStatus.Inaccessible, SubmissionStatus.InProgress]
        );

        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal(SubmissionStatus.Inaccessible, result.Status);
    }

    [Fact]
    public async Task SetSubmissionReviewed_Throws_When_Submission_Not_Found()
    {
        using var db = BuildPlanTechDbContext(
            nameof(SetSubmissionReviewed_Throws_When_Submission_Not_Found)
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(999)
        );
    }

    [Fact]
    public async Task SetSubmissionInaccessible_ById_Throws_When_NotFound()
    {
        using var db = BuildPlanTechDbContext(
            nameof(SetSubmissionInaccessible_ById_Throws_When_NotFound)
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.SetSubmissionInaccessibleAsync(12345)
        );
    }

    [Fact]
    public async Task SetSubmissionInaccessible_ByScope_Throws_When_NotFound()
    {
        using var db = BuildPlanTechDbContext(
            nameof(SetSubmissionInaccessible_ByScope_Throws_When_NotFound)
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.SetSubmissionInaccessibleAsync(establishmentId: 99, sectionId: "NA")
        );
    }

    [Fact]
    public async Task SetSubmissionInProgress_ById_Throws_When_NotFound()
    {
        using var db = BuildPlanTechDbContext(
            nameof(SetSubmissionInProgress_ById_Throws_When_NotFound)
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.SetSubmissionInProgressAsync(12345)
        );
    }

    [Fact]
    public async Task SetSubmissionInProgress_ByScope_Throws_When_NotFound()
    {
        using var db = BuildPlanTechDbContext(
            nameof(SetSubmissionInProgress_ByScope_Throws_When_NotFound)
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.SetSubmissionInProgressAsync(establishmentId: 99, sectionId: "NA")
        );
    }

    [Fact]
    public async Task GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync_Returns_Latest_Completed_NotDeleted_Submissions_Per_Establishment_And_Section()
    {
        using var db = BuildPlanTechDbContext(
            nameof(
                GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync_Returns_Latest_Completed_NotDeleted_Submissions_Per_Establishment_And_Section
            )
        );
        var repo = new SubmissionRepository(db, BuildUserActionIdAccessor());

        db.Establishments.AddRange(
            BuildEstablishmentEntity(1),
            BuildEstablishmentEntity(2),
            BuildEstablishmentEntity(3)
        );

        db.Submissions.AddRange(
            // Establishment 1, Section B - older completed submission, should be excluded
            new SubmissionEntity
            {
                Id = 1,
                EstablishmentId = 1,
                SectionId = "SEC-B",
                SectionName = "Section B",
                Status = SubmissionStatus.CompleteReviewed,
                DateCreated = new DateTime(2024, 1, 1),
                DateCompleted = new DateTime(2024, 1, 1),
                DateLastUpdated = new DateTime(2024, 1, 1),
                Deleted = false,
            },
            // Establishment 1, Section B - latest valid completed submission, should be returned
            new SubmissionEntity
            {
                Id = 2,
                EstablishmentId = 1,
                SectionId = "SEC-B",
                SectionName = "Section B",
                Status = SubmissionStatus.CompleteReviewed,
                DateCreated = new DateTime(2024, 2, 1),
                DateCompleted = new DateTime(2024, 2, 1),
                DateLastUpdated = new DateTime(2024, 2, 1),
                Deleted = false,
            },
            // Establishment 1, Section A - different section, should also be returned
            new SubmissionEntity
            {
                Id = 3,
                EstablishmentId = 1,
                SectionId = "SEC-A",
                SectionName = "Section A",
                Status = SubmissionStatus.CompleteReviewed,
                DateCreated = new DateTime(2024, 3, 1),
                DateCompleted = new DateTime(2024, 3, 1),
                DateLastUpdated = new DateTime(2024, 3, 1),
                Deleted = false,
            },
            // Newer but deleted, should be excluded and should not replace Id 2
            new SubmissionEntity
            {
                Id = 4,
                EstablishmentId = 1,
                SectionId = "SEC-B",
                SectionName = "Section B",
                Status = SubmissionStatus.CompleteReviewed,
                DateCreated = new DateTime(2024, 4, 1),
                DateCompleted = new DateTime(2024, 4, 1),
                DateLastUpdated = new DateTime(2024, 4, 1),
                Deleted = true,
            },
            // CompleteReviewed but no DateCompleted, should be excluded
            new SubmissionEntity
            {
                Id = 5,
                EstablishmentId = 1,
                SectionId = "SEC-C",
                SectionName = "Section C",
                Status = SubmissionStatus.CompleteReviewed,
                DateCreated = new DateTime(2024, 5, 1),
                DateCompleted = null,
                DateLastUpdated = new DateTime(2024, 5, 1),
                Deleted = false,
            },
            // Not CompleteReviewed, should be excluded
            new SubmissionEntity
            {
                Id = 6,
                EstablishmentId = 1,
                SectionId = "SEC-D",
                SectionName = "Section D",
                Status = SubmissionStatus.InProgress,
                DateCreated = new DateTime(2024, 6, 1),
                DateCompleted = new DateTime(2024, 6, 1),
                DateLastUpdated = new DateTime(2024, 6, 1),
                Deleted = false,
            },
            // Establishment 2, should be returned
            new SubmissionEntity
            {
                Id = 7,
                EstablishmentId = 2,
                SectionId = "SEC-A",
                SectionName = "Section A",
                Status = SubmissionStatus.CompleteReviewed,
                DateCreated = new DateTime(2024, 7, 1),
                DateCompleted = new DateTime(2024, 7, 1),
                DateLastUpdated = new DateTime(2024, 7, 1),
                Deleted = false,
            },
            // Establishment 3 is not requested, should be excluded
            new SubmissionEntity
            {
                Id = 8,
                EstablishmentId = 3,
                SectionId = "SEC-A",
                SectionName = "Section A",
                Status = SubmissionStatus.CompleteReviewed,
                DateCreated = new DateTime(2024, 8, 1),
                DateCompleted = new DateTime(2024, 8, 1),
                DateLastUpdated = new DateTime(2024, 8, 1),
                Deleted = false,
            }
        );

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await repo.GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync([
            1,
            1,
            2,
        ]);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(3, submission.Id);
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal("SEC-A", submission.SectionId);
                Assert.Equal("Section A", submission.SectionName);
            },
            submission =>
            {
                Assert.Equal(2, submission.Id);
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal("SEC-B", submission.SectionId);
                Assert.Equal("Section B", submission.SectionName);
            },
            submission =>
            {
                Assert.Equal(7, submission.Id);
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal("SEC-A", submission.SectionId);
                Assert.Equal("Section A", submission.SectionName);
            }
        );
    }
}
