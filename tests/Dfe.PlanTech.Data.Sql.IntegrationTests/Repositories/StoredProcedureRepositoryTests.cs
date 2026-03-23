using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class StoredProcedureRepositoryTests : DatabaseIntegrationTestBase
{
    private StoredProcedureRepository _repository = null!;

    public StoredProcedureRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new StoredProcedureRepository(DbContext);
    }

    private static EstablishmentEntity CreateEstablishment(int id, string? name = null)
    {
        return new EstablishmentEntity
        {
            EstablishmentRef = $"TEST{id.ToString()}",
            OrgName = name ?? $"Test Establishment {id}",
        };
    }

    private static UserEntity CreateUser(int id)
    {
        return new UserEntity
        {
            DfeSignInRef = $"User{id}",
        };
    }

    private static QuestionEntity CreateQuestion(int id)
    {
        return new QuestionEntity
        {
            ContentfulRef = $"Q{id.ToString()}",
            QuestionText = $"Question {id}",
        };
    }

    private static AnswerEntity CreateAnswer(int id)
    {
        return new AnswerEntity
        {
            ContentfulRef = $"A{id.ToString()}",
            AnswerText = $"Answer {id}",
        };
    }

    [Fact]
    public async Task StoredProcedureRepository_GetFirstActivityForEstablishmentRecommendationAsync_WhenNoMatchingHistoryExists_ThenReturnsNull()
    {
        var result = await _repository.GetFirstActivityForEstablishmentRecommendationAsync(
            999999,
            "REC-NOT-FOUND"
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task StoredProcedureRepository_GetFirstActivityForEstablishmentRecommendationAsync_WhenMatchingHistoryExists_ThenReturnsExpectedFirstActivity()
    {
        var school = CreateEstablishment(101, "School 101");
        var mat = CreateEstablishment(102, "Group 102");
        var user = CreateUser(201);
        var question = CreateQuestion(301);
        var answer = CreateAnswer(401);

        DbContext.Establishments.AddRange(school, mat);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            ContentfulRef = "REC001",
            RecommendationText = "Recommendation 1",
            QuestionId = question.Id,
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        await DbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO migration.sectionRecommendations (sectionRef, recommendationRef)
            VALUES ({0}, {1})
            """,
            "S001",
            recommendation.ContentfulRef
        );

        var submission = new SubmissionEntity
        {
            EstablishmentId = school.Id,
            SectionId = "S001",
            SectionName = "Section 1",
            Status = SubmissionStatus.CompleteReviewed,
            DateCreated = DateTime.UtcNow,
        };

        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync();

        var response = new ResponseEntity
        {
            UserId = user.Id,
            UserEstablishmentId = school.Id,
            SubmissionId = submission.Id,
            QuestionId = question.Id,
            AnswerId = answer.Id,
            Maturity = string.Empty,
        };

        DbContext.Responses.Add(response);

        var fixedDate = new DateTime(2026, 03, 22, 15, 34, 47, 990, DateTimeKind.Utc);

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = school.Id,
            MatEstablishmentId = mat.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.NotStarted.ToString(),
            DateCreated = fixedDate,
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetFirstActivityForEstablishmentRecommendationAsync(
            school.Id,
            recommendation.ContentfulRef
        );

        Assert.NotNull(result);
        Assert.Equal(fixedDate, result!.StatusChangeDate);
        Assert.Equal(history.NewStatus, result.StatusText);
        Assert.Equal(school.OrgName, result.SchoolName);
        Assert.Equal(mat.OrgName, result.GroupName);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(question.QuestionText, result.QuestionText);
        Assert.Equal(answer.AnswerText, result.AnswerText);
    }

    [Fact]
    public async Task StoredProcedureRepository_GetFirstActivityForEstablishmentRecommendationAsync_WhenMultipleMatchingHistoriesExist_ThenReturnsLatestByHistoryId()
    {
        var school = CreateEstablishment(111, "School 111");
        var user = CreateUser(211);
        var question = CreateQuestion(311);
        var answer = CreateAnswer(411);

        DbContext.Establishments.Add(school);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            ContentfulRef = "REC002",
            RecommendationText = "Recommendation 2",
            QuestionId = question.Id,
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        await DbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO migration.sectionRecommendations (sectionRef, recommendationRef)
            VALUES ({0}, {1})
            """,
            "S002",
            recommendation.ContentfulRef
        );

        var submission = new SubmissionEntity
        {
            EstablishmentId = school.Id,
            SectionId = "S002",
            SectionName = "Section 2",
            Status = SubmissionStatus.CompleteReviewed,
            DateCreated = DateTime.UtcNow,
        };

        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync();

        var response = new ResponseEntity
        {
            UserId = user.Id,
            UserEstablishmentId = school.Id,
            SubmissionId = submission.Id,
            QuestionId = question.Id,
            AnswerId = answer.Id,
            Maturity = string.Empty,
        };

        DbContext.Responses.Add(response);
        await DbContext.SaveChangesAsync();

        var firstDate = new DateTime(2026, 03, 21, 12, 0, 0, 0, DateTimeKind.Utc);
        var latestDate = new DateTime(2026, 03, 22, 15, 34, 47, 990, DateTimeKind.Utc);

        var firstHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = school.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.NotStarted.ToString(),
            DateCreated = firstDate,
        };

        DbContext.EstablishmentRecommendationHistories.Add(firstHistory);
        await DbContext.SaveChangesAsync();

        var latestHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = school.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.InProgress.ToString(),
            DateCreated = latestDate,
        };

        DbContext.EstablishmentRecommendationHistories.Add(latestHistory);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetFirstActivityForEstablishmentRecommendationAsync(
            school.Id,
            recommendation.ContentfulRef
        );

        Assert.NotNull(result);
        Assert.Equal(latestDate, result!.StatusChangeDate);
        Assert.Equal(latestHistory.NewStatus, result.StatusText);
    }
}
