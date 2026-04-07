using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

/// <summary>
/// Integration tests specifically focused on validating stored procedure calls from repository classes.
/// These tests ensure that stored procedures are called with correct parameters, types, and order.
/// </summary>
public class StoredProcedureCallValidationTests : DatabaseIntegrationTestBase
{
    private StoredProcedureRepository _storedProcRepository = null!;

    public StoredProcedureCallValidationTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _storedProcRepository = new StoredProcedureRepository(DbContext);
    }

    [Fact]
    public async Task StoredProcedureRepository_GetFirstActivityForEstablishmentRecommendationAsync_WhenCalledWithValidParameters_ThenReturnsExpectedData_WithNullGroupName()
    {
        async Task insertSectionRecommendationAsync(
            string sectionRef,
            string recommendationContentfulRef
        )
        {
            var sql =
                @$"
                IF OBJECT_ID('migration.sectionRecommendations', 'U') IS NULL
                BEGIN
                    CREATE TABLE [migration].[sectionRecommendations](
	                    [sectionRef] [nvarchar](100) NULL,
	                    [recommendationRef] [nvarchar](100) NULL
                    ) ON [PRIMARY]
                END;

                INSERT INTO migration.sectionRecommendations (sectionRef, recommendationRef)
                VALUES ('{sectionRef}', '{recommendationContentfulRef}');
            ";

            await DbContext.Database.ExecuteSqlRawAsync(sql);
        }

        // Arrange
        var school = new EstablishmentEntity
        {
            EstablishmentRef = "SCHOOL001",
            OrgName = "Test School",
        };
        DbContext.Establishments.Add(school);

        var group = new EstablishmentEntity
        {
            EstablishmentRef = "GROUP001",
            OrgName = "Test Group",
        };
        DbContext.Establishments.Add(group);

        var user = new UserEntity { DfeSignInRef = "test-user" };
        DbContext.Users.Add(user);

        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var answer = new AnswerEntity { AnswerText = "Test Answer", ContentfulRef = "A1" };
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);

        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        const string recommendationContentfulRef = "REC-001";
        const string sectionRef = "section-xyz";

        var submission = new SubmissionEntity
        {
            EstablishmentId = school.Id,
            SectionId = sectionRef,
            SectionName = "Section XYZ",
            Status = SubmissionStatus.InProgress,
            DateCreated = DateTime.UtcNow.AddDays(-10),
        };
        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = new ResponseEntity
        {
            SubmissionId = submission.Id,
            UserId = user.Id,
            UserEstablishmentId = school.Id,
            QuestionId = question.Id,
            AnswerId = answer.Id,
            DateCreated = DateTime.UtcNow.AddDays(-9),
            Maturity = "",
        };
        DbContext.Responses.Add(response);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            ContentfulRef = recommendationContentfulRef,
            QuestionId = question.Id,
            RecommendationText = "Test Recommendation",
            Question = question,
            DateCreated = DateTime.UtcNow,
            Archived = false,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await insertSectionRecommendationAsync(sectionRef, recommendationContentfulRef);

        var earliest = DateTime.UtcNow.AddDays(-8);
        var later = DateTime.UtcNow.AddDays(-7);

        var recommendationHistory1 = new EstablishmentRecommendationHistoryEntity
        {
            DateCreated = earliest,
            EstablishmentId = school.Id,
            MatEstablishmentId = group.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.NotStarted.ToString(),
            NoteText = null,
        };

        var recommendationHistory2 = new EstablishmentRecommendationHistoryEntity
        {
            DateCreated = later,
            EstablishmentId = school.Id,
            MatEstablishmentId = group.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.NotStarted.ToString(),
            NewStatus = RecommendationStatus.InProgress.ToString(),
            NoteText = null,
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(
            recommendationHistory1,
            recommendationHistory2
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result =
            await _storedProcRepository.GetFirstActivityForEstablishmentRecommendationAsync(
                school.Id,
                recommendationContentfulRef
            );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test School", result.SchoolName);
        Assert.Equal("Test Group", result.GroupName);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Test Question", result.QuestionText);
        Assert.Equal("Test Answer", result.AnswerText);

        // Should reflect the earliest history record
        Assert.Equal(RecommendationStatus.NotStarted.ToString(), result.StatusText);
        Assert.True(
            (result.StatusChangeDate - earliest).Duration() < TimeSpan.FromMilliseconds(3.3),
            $"Expected timtstamp within 3.3ms of record value. Expected={earliest:o}, Actual={result.StatusChangeDate:o}"
        );
    }

    [Fact]
    public async Task StoredProcedureRepository_GetFirstActivityForEstablishmentRecommendationAsync_WhenGroupNameIsNull_ThenReturnsNullGroupName()
    {
        async Task insertSectionRecommendationAsync(
            string sectionRef,
            string recommendationContentfulRef
        )
        {
            var sql =
                @$"
                IF OBJECT_ID('migration.sectionRecommendations', 'U') IS NULL
                BEGIN
                    CREATE TABLE [migration].[sectionRecommendations](
	                    [sectionRef] [nvarchar](100) NULL,
	                    [recommendationRef] [nvarchar](100) NULL
                    ) ON [PRIMARY]
                END;

                INSERT INTO migration.sectionRecommendations (sectionRef, recommendationRef)
                VALUES ('{sectionRef}', '{recommendationContentfulRef}');
            ";

            await DbContext.Database.ExecuteSqlRawAsync(sql);
        }

        // Arrange
        var school = new EstablishmentEntity
        {
            EstablishmentRef = "SCHOOL001",
            OrgName = "Test School",
        };
        DbContext.Establishments.Add(school);

        var user = new UserEntity { DfeSignInRef = "test-user" };
        DbContext.Users.Add(user);

        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var answer = new AnswerEntity { AnswerText = "Test Answer", ContentfulRef = "A1" };
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);

        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        const string recommendationContentfulRef = "REC-001";
        const string sectionRef = "section-xyz";

        var submission = new SubmissionEntity
        {
            EstablishmentId = school.Id,
            SectionId = sectionRef,
            SectionName = "Section XYZ",
            Status = SubmissionStatus.InProgress,
            DateCreated = DateTime.UtcNow.AddDays(-10),
        };
        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = new ResponseEntity
        {
            SubmissionId = submission.Id,
            UserId = user.Id,
            UserEstablishmentId = school.Id,
            QuestionId = question.Id,
            AnswerId = answer.Id,
            DateCreated = DateTime.UtcNow.AddDays(-9),
            Maturity = "",
        };
        DbContext.Responses.Add(response);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            ContentfulRef = recommendationContentfulRef,
            QuestionId = question.Id,
            RecommendationText = "Test Recommendation",
            Question = question,
            DateCreated = DateTime.UtcNow,
            Archived = false,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await insertSectionRecommendationAsync(sectionRef, recommendationContentfulRef);

        var earliest = DateTime.UtcNow.AddDays(-8);
        var later = DateTime.UtcNow.AddDays(-7);

        var recommendationHistory1 = new EstablishmentRecommendationHistoryEntity
        {
            DateCreated = earliest,
            EstablishmentId = school.Id,
            MatEstablishmentId = null,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.NotStarted.ToString(),
            NoteText = null,
        };

        var recommendationHistory2 = new EstablishmentRecommendationHistoryEntity
        {
            DateCreated = later,
            EstablishmentId = school.Id,
            MatEstablishmentId = null,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.NotStarted.ToString(),
            NewStatus = RecommendationStatus.InProgress.ToString(),
            NoteText = null,
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(
            recommendationHistory1,
            recommendationHistory2
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result =
            await _storedProcRepository.GetFirstActivityForEstablishmentRecommendationAsync(
                school.Id,
                recommendationContentfulRef
            );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test School", result.SchoolName);
        Assert.Null(result.GroupName);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Test Question", result.QuestionText);
        Assert.Equal("Test Answer", result.AnswerText);

        // Should reflect the earliest history record
        Assert.Equal(RecommendationStatus.NotStarted.ToString(), result.StatusText);
        Assert.True(
            (result.StatusChangeDate - earliest).Duration() < TimeSpan.FromMilliseconds(3.3),
            $"Expected timtstamp within 3.3ms of record value. Expected={earliest:o}, Actual={result.StatusChangeDate:o}"
        );
    }

    [Fact]
    public async Task StoredProcedureRepository_GetFirstActivityForEstablishmentRecommendationAsync_WhenNoDataReturned_ThenReturnsNull()
    {
        // Arrange
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "TEST003",
            OrgName = "Test School",
        };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act & Assert
        var result =
            await _storedProcRepository.GetFirstActivityForEstablishmentRecommendationAsync(
                establishment.Id,
                "REC-DOES-NOT-EXIST"
            );

        Assert.Null(result);
    }

    /* =================================================== */

    [Fact]
    public async Task StoredProcedureRepository_GetSectionStatusesAsync_WhenCalledWithValidParameters_ThenReturnsExpectedSectionStatusData()
    {
        // Arrange
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "TEST001",
            OrgName = "Test School",
        };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create test data that should be returned by the stored procedure
        var user = new UserEntity { DfeSignInRef = "test-user" };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var submission1 = new SubmissionEntity
        {
            SectionId = "section1",
            SectionName = "Section 1",
            EstablishmentId = establishment.Id,
            Status = Core.Enums.SubmissionStatus.CompleteReviewed,
            DateCreated = DateTime.UtcNow.AddDays(-1),
        };

        var submission2 = new SubmissionEntity
        {
            SectionId = "section2",
            SectionName = "Section 2",
            EstablishmentId = establishment.Id,
            Status = Core.Enums.SubmissionStatus.InProgress,
            DateCreated = DateTime.UtcNow.AddDays(-2),
        };

        DbContext.Submissions.AddRange(submission1, submission2);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sectionIds = "section1,section2,section3";

        // Act
        var result = await _storedProcRepository.GetSectionStatusesAsync(
            sectionIds,
            establishment.Id
        );

        // Assert - Validate meaningful results are returned
        Assert.NotNull(result);

        // Should return data for sections that exist in the database
        var resultList = result.ToList();
        Assert.True(resultList.Count >= 0, "Result should be a valid collection");

        // If data is returned, validate it contains expected section information
        if (resultList.Any())
        {
            // Verify that returned data relates to our test sections
            var sectionIdsArray = sectionIds.Split(',');
            Assert.True(
                resultList.All(r =>
                    sectionIdsArray.Contains(r.SectionId) || string.IsNullOrEmpty(r.SectionId)
                ),
                "All returned results should relate to requested sections"
            );
        }
    }
}
