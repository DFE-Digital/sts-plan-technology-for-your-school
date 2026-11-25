using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

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
        DbContext.Questions.Add(question);

        var answer = new AnswerEntity { AnswerText = "Test Answer", ContentfulRef = "A1" };
        DbContext.Answers.Add(answer);

        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        const string recommendationContentfulRef = "REC-001";
        const string sectionRef = "section-xyz";

        var submission = new SubmissionEntity
        {
            EstablishmentId = school.Id,
            SectionId = sectionRef,
            SectionName = "Section XYZ",
            Status = SubmissionStatus.CompleteReviewed,
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
        };
        DbContext.Responses.Add(response);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            ContentfulRef = recommendationContentfulRef,
            QuestionId = question.Id,
            QuestionContentfulRef = question.ContentfulRef,
            RecommendationText = "Test Recommendation",
            Question = question,
            DateCreated = DateTime.UtcNow,
            Archived = false,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
            NewStatus = RecommendationStatus.NotStarted,
            NoteText = null,
        };

        var recommendationHistory2 = new EstablishmentRecommendationHistoryEntity
        {
            DateCreated = later,
            EstablishmentId = school.Id,
            MatEstablishmentId = group.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.NotStarted,
            NewStatus = RecommendationStatus.InProgress,
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
        Assert.Equal(RecommendationStatus.NotStarted, result.Status);
        Assert.True(
            (result.StatusChangeDate - earliest).Duration() < TimeSpan.FromMilliseconds(3.3),
            $"Expected timtstamp within 3.3ms of record value. Expected={earliest:o}, Actual={result.StatusChangeDate:o}"
        );
    }

    [Fact]
    public async Task StoredProcedureRepository_GetFirstActivityForEstablishmentRecommendationAsync_WhenGroupNameIsNull_ThenReturnsNullGroupName()
    {
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
            Status = SubmissionStatus.CompleteReviewed,
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
        };
        DbContext.Responses.Add(response);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            ContentfulRef = recommendationContentfulRef,
            QuestionId = question.Id,
            QuestionContentfulRef = question.ContentfulRef,
            RecommendationText = "Test Recommendation",
            Question = question,
            DateCreated = DateTime.UtcNow,
            Archived = false,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
            NewStatus = RecommendationStatus.NotStarted,
            NoteText = null,
        };

        var recommendationHistory2 = new EstablishmentRecommendationHistoryEntity
        {
            DateCreated = later,
            EstablishmentId = school.Id,
            MatEstablishmentId = null,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.NotStarted,
            NewStatus = RecommendationStatus.InProgress,
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
        Assert.Equal(RecommendationStatus.NotStarted, result.Status);
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
}
