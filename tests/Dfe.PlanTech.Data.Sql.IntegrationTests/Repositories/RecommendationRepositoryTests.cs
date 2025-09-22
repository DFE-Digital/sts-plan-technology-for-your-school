using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class RecommendationRepositoryTests : DatabaseIntegrationTestBase
{
    private RecommendationRepository _repository = null!;

    public RecommendationRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new RecommendationRepository(DbContext);
    }

    #region Recommendation Management Tests

    [Fact]
    public async Task RecommendationRepository_CreateRecommendationAsync_WhenGivenValidRecommendation_ThenCreatesAndReturnsRecommendation()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };

        // Act
        var result = await _repository.CreateRecommendationAsync(recommendation);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test Recommendation", result.RecommendationText);
        Assert.Equal("R1", result.ContentfulRef);
        Assert.Equal(question.Id, result.QuestionId);
        Assert.False(result.Archived);

        // Verify it was saved to database
        var saved = await DbContext.Recommendations.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test Recommendation", saved!.RecommendationText);
    }

    [Fact]
    public async Task RecommendationRepository_CreateRecommendationAsync_WhenRecommendationIsNull_ThenThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.CreateRecommendationAsync(null!));
    }

    [Fact]
    public async Task RecommendationRepository_GetRecommendationByIdAsync_WhenRecommendationExists_ThenReturnsRecommendationWithRelatedData()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecommendationByIdAsync(recommendation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recommendation.Id, result!.Id);
        Assert.Equal("Test Recommendation", result.RecommendationText);
        Assert.NotNull(result.Question);
        Assert.Equal("Test Question", result.Question.QuestionText);
    }

    [Fact]
    public async Task RecommendationRepository_GetRecommendationByIdAsync_WhenRecommendationDoesNotExist_ThenReturnsNull()
    {
        // Act
        var result = await _repository.GetRecommendationByIdAsync(99999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RecommendationRepository_GetRecommendationByContentfulRefAsync_WhenRecommendationExists_ThenReturnsRecommendation()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecommendationByContentfulRefAsync("R1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recommendation.Id, result!.Id);
        Assert.Equal("R1", result.ContentfulRef);
    }

    [Fact]
    public async Task RecommendationRepository_GetRecommendationByContentfulRefAsync_WhenRecommendationDoesNotExist_ThenReturnsNull()
    {
        // Act
        var result = await _repository.GetRecommendationByContentfulRefAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RecommendationRepository_GetActiveRecommendationsByQuestionIdAsync_WhenActiveRecommendationsExist_ThenReturnsOnlyActiveRecommendations()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var activeRecommendation = new RecommendationEntity
        {
            RecommendationText = "Active Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };

        var archivedRecommendation = new RecommendationEntity
        {
            RecommendationText = "Archived Recommendation",
            ContentfulRef = "R2",
            QuestionId = question.Id,
            Archived = true
        };

        DbContext.Recommendations.AddRange(activeRecommendation, archivedRecommendation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveRecommendationsByQuestionIdAsync(question.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Active Recommendation", result[0].RecommendationText);
        Assert.False(result[0].Archived);
    }

    [Fact]
    public async Task RecommendationRepository_GetRecommendationsAsync_WhenUsingPredicate_ThenReturnsMatchingRecommendations()
    {
        // Arrange
        var question1 = new QuestionEntity { QuestionText = "Question 1", ContentfulRef = "Q1" };
        var question2 = new QuestionEntity { QuestionText = "Question 2", ContentfulRef = "Q2" };
        DbContext.Questions.AddRange(question1, question2);
        await DbContext.SaveChangesAsync();

        var rec1 = new RecommendationEntity
        {
            RecommendationText = "Recommendation 1 for Q1",
            ContentfulRef = "R1",
            QuestionId = question1.Id,
            Archived = false
        };

        var rec2 = new RecommendationEntity
        {
            RecommendationText = "Recommendation 2 for Q1",
            ContentfulRef = "R2",
            QuestionId = question1.Id,
            Archived = true
        };

        var rec3 = new RecommendationEntity
        {
            RecommendationText = "Recommendation for Q2",
            ContentfulRef = "R3",
            QuestionId = question2.Id,
            Archived = false
        };

        DbContext.Recommendations.AddRange(rec1, rec2, rec3);
        await DbContext.SaveChangesAsync();

        // Act - Get all recommendations for question1 (both active and archived)
        var result = await _repository.GetRecommendationsAsync(r => r.QuestionId == question1.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.ContentfulRef == "R1");
        Assert.Contains(result, r => r.ContentfulRef == "R2");
        Assert.DoesNotContain(result, r => r.ContentfulRef == "R3");
    }

    [Fact]
    public async Task RecommendationRepository_UpdateRecommendationAsync_WhenRecommendationExists_ThenUpdatesRecommendation()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Original Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Act
        recommendation.RecommendationText = "Updated Recommendation";
        var result = await _repository.UpdateRecommendationAsync(recommendation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Recommendation", result.RecommendationText);

        // Verify it was updated in database
        var updated = await _repository.GetRecommendationByIdAsync(recommendation.Id);
        Assert.Equal("Updated Recommendation", updated!.RecommendationText);
    }

    [Fact]
    public async Task RecommendationRepository_UpdateRecommendationAsync_WhenRecommendationIsNull_ThenThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.UpdateRecommendationAsync(null!));
    }

    [Fact]
    public async Task RecommendationRepository_ArchiveRecommendationAsync_WhenRecommendationExists_ThenMarksRecommendationAsArchived()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Act
        await _repository.ArchiveRecommendationAsync(recommendation.Id);

        // Assert
        var archived = await _repository.GetRecommendationByIdAsync(recommendation.Id);
        Assert.NotNull(archived);
        Assert.True(archived!.Archived);
    }

    [Fact]
    public async Task RecommendationRepository_ArchiveRecommendationAsync_WhenRecommendationDoesNotExist_ThenThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.ArchiveRecommendationAsync(99999));

        Assert.Contains("Recommendation not found for ID '99999'", exception.Message);
    }

    #endregion

    #region Establishment Recommendation Status Management Tests

    [Fact]
    public async Task RecommendationRepository_UpdateRecommendationStatusAsync_WhenFirstStatusUpdate_ThenCreatesNewHistoryRecord()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "user123" };

        // Save questions first to get their IDs and satisfy foreign key constraints
        DbContext.Questions.Add(question);
        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation.Id, user.Id, "Started", "Initial setup note");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(establishment.Id, result.EstablishmentId);
        Assert.Equal(recommendation.Id, result.RecommendationId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Null(result.PreviousStatus); // First time, so no previous status
        Assert.Equal("Started", result.NewStatus);
        Assert.Equal("Initial setup note", result.NoteText);
        Assert.True(result.DateCreated >= beforeUpdate);

        // Verify it was saved to database
        var savedHistory = await _repository.GetCurrentRecommendationStatusAsync(establishment.Id, recommendation.Id);
        Assert.NotNull(savedHistory);
        Assert.Equal("Started", savedHistory!.NewStatus);
    }

    [Fact]
    public async Task RecommendationRepository_UpdateRecommendationStatusAsync_WhenUpdatingExistingStatus_ThenCreatesNewHistoryRecordWithPreviousStatus()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user1 = new UserEntity { DfeSignInRef = "user123" };
        var user2 = new UserEntity { DfeSignInRef = "user456" };

        // Save entities first to get their IDs and satisfy foreign key constraints
        DbContext.Questions.Add(question);
        DbContext.Establishments.Add(establishment);
        DbContext.Users.AddRange(user1, user2);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Create initial status
        await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation.Id, user1.Id, "Started");

        // Act - Update status
        var result = await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation.Id, user2.Id, "InProgress", "Made some progress");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Started", result.PreviousStatus); // Previous status should be captured
        Assert.Equal("InProgress", result.NewStatus);
        Assert.Equal(user2.Id, result.UserId);
        Assert.Equal("Made some progress", result.NoteText);
    }

    [Fact]
    public async Task RecommendationRepository_GetCurrentRecommendationStatusesByEstablishmentAsync_WhenMultipleStatusesExist_ThenReturnsLatestStatusForEachRecommendation()
    {
        // Arrange
        var question1 = new QuestionEntity { QuestionText = "Question 1", ContentfulRef = "Q1" };
        var question2 = new QuestionEntity { QuestionText = "Question 2", ContentfulRef = "Q2" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "user123" };

        // Save entities first to get their IDs and satisfy foreign key constraints
        DbContext.Questions.AddRange(question1, question2);
        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var recommendation1 = new RecommendationEntity
        {
            RecommendationText = "Recommendation 1",
            ContentfulRef = "R1",
            QuestionId = question1.Id,
            Archived = false
        };

        var recommendation2 = new RecommendationEntity
        {
            RecommendationText = "Recommendation 2",
            ContentfulRef = "R2",
            QuestionId = question2.Id,
            Archived = false
        };

        DbContext.Recommendations.AddRange(recommendation1, recommendation2);
        await DbContext.SaveChangesAsync();

        // Create multiple statuses for recommendation1 to test that only latest is returned
        await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation1.Id, user.Id, "Started");
        await Task.Delay(100); // Ensure different timestamps
        await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation1.Id, user.Id, "InProgress");

        // Single status for recommendation2
        await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation2.Id, user.Id, "Completed");

        // Act
        var result = await _repository.GetCurrentRecommendationStatusesByEstablishmentAsync(establishment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // Should only get the latest status for each recommendation
        var rec1Status = result.First(r => r.RecommendationId == recommendation1.Id);
        var rec2Status = result.First(r => r.RecommendationId == recommendation2.Id);

        Assert.Equal("InProgress", rec1Status.NewStatus); // Latest status for rec1
        Assert.Equal("Completed", rec2Status.NewStatus); // Only status for rec2

        // Verify ordering by question ID
        Assert.True(result[0].Recommendation.QuestionId <= result[1].Recommendation.QuestionId);
    }

    [Fact]
    public async Task RecommendationRepository_GetCurrentRecommendationStatusAsync_WhenMultipleHistoryRecordsExist_ThenReturnsMostRecentStatus()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "user123" };

        // Save questions first to get their IDs and satisfy foreign key constraints
        DbContext.Questions.Add(question);
        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Create multiple status updates
        await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation.Id, user.Id, "Started");
        await Task.Delay(100); // Ensure different timestamps
        await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation.Id, user.Id, "InProgress");
        await Task.Delay(100);
        await _repository.UpdateRecommendationStatusAsync(
            establishment.Id, recommendation.Id, user.Id, "Completed");

        // Act
        var result = await _repository.GetCurrentRecommendationStatusAsync(establishment.Id, recommendation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Completed", result!.NewStatus); // Should return the most recent status
        Assert.Equal("InProgress", result.PreviousStatus);
    }

    [Fact]
    public async Task RecommendationRepository_GetCurrentRecommendationStatusAsync_WhenNoStatusExists_ThenReturnsNull()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };

        // Save entities first to get their IDs and satisfy foreign key constraints
        DbContext.Questions.Add(question);
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetCurrentRecommendationStatusAsync(establishment.Id, recommendation.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RecommendationRepository_GetRecommendationStatusHistoryAsync_WhenUsingPredicate_ThenReturnsMatchingHistoryRecords()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var establishment1 = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "School 1" };
        var establishment2 = new EstablishmentEntity { EstablishmentRef = "EST002", OrgName = "School 2" };
        var user = new UserEntity { DfeSignInRef = "user123" };

        // Save entities first to get their IDs and satisfy foreign key constraints
        DbContext.Questions.Add(question);
        DbContext.Establishments.AddRange(establishment1, establishment2);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Create statuses for both establishments
        await _repository.UpdateRecommendationStatusAsync(
            establishment1.Id, recommendation.Id, user.Id, "Started");
        await _repository.UpdateRecommendationStatusAsync(
            establishment2.Id, recommendation.Id, user.Id, "Completed");
        await _repository.UpdateRecommendationStatusAsync(
            establishment1.Id, recommendation.Id, user.Id, "InProgress");

        // Act - Get all history for establishment1
        var result = await _repository.GetRecommendationStatusHistoryAsync(
            h => h.EstablishmentId == establishment1.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // establishment1 has 2 history records
        Assert.All(result, r => Assert.Equal(establishment1.Id, r.EstablishmentId));

        // Should be ordered by DateCreated DESC (most recent first)
        Assert.True(result[0].DateCreated >= result[1].DateCreated);
        Assert.Equal("InProgress", result[0].NewStatus); // Most recent
        Assert.Equal("Started", result[1].NewStatus); // Older
    }

    [Fact]
    public async Task RecommendationRepository_UpdateRecommendationStatusAsync_WhenNewStatusIsNull_ThenThrowsArgumentNullException()
    {
        // Arrange
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "user123" };

        // Save entities first to get their IDs and satisfy foreign key constraints
        DbContext.Questions.Add(question);
        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.UpdateRecommendationStatusAsync(
                establishment.Id, recommendation.Id, user.Id, null!, "note"));
    }


    #endregion
}
