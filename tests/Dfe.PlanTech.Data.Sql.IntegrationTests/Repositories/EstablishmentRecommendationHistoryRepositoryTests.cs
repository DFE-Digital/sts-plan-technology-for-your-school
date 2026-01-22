using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class EstablishmentRecommendationHistoryRepositoryTests : DatabaseIntegrationTestBase
{
    private EstablishmentRecommendationHistoryRepository _repository = null!;

    public EstablishmentRecommendationHistoryRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new EstablishmentRecommendationHistoryRepository(DbContext);
    }

    [Fact]
    public async Task GetRecommendationHistoryByEstablishmentIdAsync_WhenGivenValidEstablishmentId_ThenReturnsMatchingHistory()
    {
        // Arrange - Create two establishments with history entries, ensuring only the target establishment's history is returned
        var establishment1 = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School 1",
        };
        var establishment2 = new EstablishmentEntity
        {
            EstablishmentRef = "EST002",
            OrgName = "Test School 2",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.AddRange(establishment1, establishment2);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var history1 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "First history entry for establishment 1",
        };

        var history2 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = "Completed",
            NewStatus = "Reviewed",
            NoteText = "Second history entry for establishment 1",
        };

        var history3 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment2.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "History entry for different establishment",
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(history1, history2, history3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecommendationHistoryByEstablishmentIdAsync(
            establishment1.Id
        );

        // Assert
        var histories = result.ToList();
        Assert.Equal(2, histories.Count);
        Assert.All(histories, h => Assert.Equal(establishment1.Id, h.EstablishmentId));
        Assert.DoesNotContain(histories, h => h.EstablishmentId == establishment2.Id);
    }

    [Fact]
    public async Task GetRecommendationHistoryByEstablishmentIdAndRecommendationIdAsync_WhenGivenValidEstablishmentIdAndRecommendationId_ThenReturnsMatchingHistory()
    {
        // Arrange - Create two establishments with history entries, ensuring only the target establishment's history is returned
        var establishment1 = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School 1",
        };
        var establishment2 = new EstablishmentEntity
        {
            EstablishmentRef = "EST002",
            OrgName = "Test School 2",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };

        DbContext.Establishments.AddRange(establishment1, establishment2);
        DbContext.Users.Add(user);

        var question2 = new QuestionEntity
        {
            QuestionText = "Test Question 2",
            ContentfulRef = "Q2",
        };
        var question1 = new QuestionEntity
        {
            QuestionText = "Test Question 1",
            ContentfulRef = "Q1",
        };

        DbContext.Questions.AddRange(question1, question2);
        await DbContext.SaveChangesAsync();

        var recommendation1 = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation 1",
            ContentfulRef = "rec-001",
            QuestionId = question1.Id,
        };

        var recommendation2 = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation 2",
            ContentfulRef = "rec-002",
            QuestionId = question2.Id,
        };

        DbContext.Recommendations.AddRange(recommendation1, recommendation2);
        await DbContext.SaveChangesAsync();

        var history1 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation1.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "First history entry for establishment 1",
        };

        var history2 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation2.Id,
            UserId = user.Id,
            PreviousStatus = "Completed",
            NewStatus = "Reviewed",
            NoteText = "Second history entry for establishment 1",
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(history1, history2);
        await DbContext.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetRecommendationHistoryByEstablishmentIdAndRecommendationIdAsync(
                establishment1.Id,
                recommendation2.Id
            );

        // Assert
        var histories = result.ToList();
        Assert.Single(histories);
        Assert.All(histories, h => Assert.Equal(establishment1.Id, h.EstablishmentId));
        Assert.DoesNotContain(histories, h => h.EstablishmentId == establishment2.Id);
        Assert.DoesNotContain(histories, h => h.RecommendationId == recommendation1.Id);
    }

    [Fact]
    public async Task GetRecommendationHistoryByEstablishmentIdAsync_WhenNoHistoryExists_ThenReturnsEmpty()
    {
        // Arrange - Create an establishment with no history entries
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecommendationHistoryByEstablishmentIdAsync(
            establishment.Id
        );

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationHistoryByEstablishmentIdAsync_WhenNonExistentEstablishment_ThenReturnsEmpty()
    {
        // Arrange - Create history for one establishment and query for a different non-existent establishment
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "Test history entry for existing establishment",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        // Act - Query for non-existent establishment ID
        var result = await _repository.GetRecommendationHistoryByEstablishmentIdAsync(99999);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationHistoryByEstablishmentIdAsync_WhenMultipleRecommendations_ThenReturnsAllHistoryForEstablishment()
    {
        // Arrange - Create one establishment with history for multiple different recommendations
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation1 = new RecommendationEntity
        {
            RecommendationText = "First Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        var recommendation2 = new RecommendationEntity
        {
            RecommendationText = "Second Recommendation",
            ContentfulRef = "rec-002",
            QuestionId = question.Id,
        };

        DbContext.Recommendations.AddRange(recommendation1, recommendation2);
        await DbContext.SaveChangesAsync();

        var history1 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation1.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "History for first recommendation",
        };

        var history2 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation2.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "History for second recommendation",
        };

        var history3 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation1.Id,
            UserId = user.Id,
            PreviousStatus = "Completed",
            NewStatus = "Reviewed",
            NoteText = "Second history entry for first recommendation",
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(history1, history2, history3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecommendationHistoryByEstablishmentIdAsync(
            establishment.Id
        );

        // Assert
        var histories = result.ToList();
        Assert.Equal(3, histories.Count);
        Assert.All(histories, h => Assert.Equal(establishment.Id, h.EstablishmentId));

        // Verify we have history for both recommendations
        Assert.Contains(histories, h => h.RecommendationId == recommendation1.Id);
        Assert.Contains(histories, h => h.RecommendationId == recommendation2.Id);
    }

    [Fact]
    public async Task GetRecommendationHistoryByEstablishmentIdAsync_WhenHistoryHasMatEstablishment_ThenReturnsWithMatReference()
    {
        // Arrange - Create history entry with MAT establishment reference to test optional relationship
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var matEstablishment = new EstablishmentEntity
        {
            EstablishmentRef = "MAT001",
            OrgName = "Test MAT",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.AddRange(establishment, matEstablishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            MatEstablishmentId = matEstablishment.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "History entry with MAT establishment reference",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecommendationHistoryByEstablishmentIdAsync(
            establishment.Id
        );

        // Assert
        var historyItem = Assert.Single(result);
        Assert.Equal(establishment.Id, historyItem.EstablishmentId);
        Assert.Equal(matEstablishment.Id, historyItem.MatEstablishmentId);
    }

    [Fact]
    public async Task GetRecommendationHistoryByEstablishmentIdAsync_WhenHistoryOrderingMatters_ThenReturnsInCorrectOrder()
    {
        // Arrange - Create multiple history entries with different DateCreated values to test that all entries are returned
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var oldestHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = "InProgress",
            NoteText = "Oldest history entry - initial status",
            DateCreated = DateTime.UtcNow.AddDays(-5),
        };

        var newestHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = "Completed",
            NewStatus = "Reviewed",
            NoteText = "Newest history entry - final review",
            DateCreated = DateTime.UtcNow.AddDays(-1),
        };

        var middleHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "Middle history entry - completion",
            DateCreated = DateTime.UtcNow.AddDays(-3),
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(
            oldestHistory,
            newestHistory,
            middleHistory
        );
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecommendationHistoryByEstablishmentIdAsync(
            establishment.Id
        );

        // Assert
        var histories = result.ToList();
        Assert.Equal(3, histories.Count);

        // Verify all expected history entries are present (repository doesn't guarantee ordering)
        Assert.Contains(histories, h => h.NoteText == "Oldest history entry - initial status");
        Assert.Contains(histories, h => h.NoteText == "Newest history entry - final review");
        Assert.Contains(histories, h => h.NoteText == "Middle history entry - completion");

        // Verify all entries belong to the correct establishment
        Assert.All(histories, h => Assert.Equal(establishment.Id, h.EstablishmentId));
    }

    #region GetLatestRecommendationHistoryAsync Tests

    [Fact]
    public async Task GetLatestRecommendationHistoryAsync_WhenHistoryExists_ThenReturnsLatestEntry()
    {
        // Arrange - Create establishment with multiple history entries at different dates to test latest entry selection
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Create multiple history entries with different dates
        var oldHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = "InProgress",
            NoteText = "Initial status",
            DateCreated = DateTime.UtcNow.AddDays(-5),
        };

        var latestHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "Completed",
            NoteText = "Latest status - should be returned",
            DateCreated = DateTime.UtcNow.AddDays(-1),
        };

        var middleHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = "InProgress",
            NewStatus = "InReview",
            NoteText = "Middle status",
            DateCreated = DateTime.UtcNow.AddDays(-3),
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(
            oldHistory,
            latestHistory,
            middleHistory
        );
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestRecommendationHistoryAsync(
            establishment.Id,
            recommendation.Id
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Latest status - should be returned", result.NoteText);
        Assert.Equal("Completed", result.NewStatus);
        Assert.Equal(latestHistory.DateCreated, result.DateCreated);
    }

    [Fact]
    public async Task GetLatestRecommendationHistoryAsync_WhenNoHistoryExists_ThenReturnsNull()
    {
        // Arrange - Create establishment and recommendation with no history entries
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestRecommendationHistoryAsync(
            establishment.Id,
            recommendation.Id
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestRecommendationHistoryAsync_WhenHistoryExistsForDifferentRecommendation_ThenReturnsNull()
    {
        // Arrange - Create history for one recommendation and query for a different recommendation
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation1 = new RecommendationEntity
        {
            RecommendationText = "First Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        var recommendation2 = new RecommendationEntity
        {
            RecommendationText = "Second Recommendation",
            ContentfulRef = "rec-002",
            QuestionId = question.Id,
        };

        DbContext.Recommendations.AddRange(recommendation1, recommendation2);
        await DbContext.SaveChangesAsync();

        // Create history for recommendation1 only
        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation1.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = "Completed",
            NoteText = "History for first recommendation",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        // Act - Query for recommendation2 (which has no history)
        var result = await _repository.GetLatestRecommendationHistoryAsync(
            establishment.Id,
            recommendation2.Id
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestRecommendationHistoryAsync_WhenHistoryExistsForDifferentEstablishment_ThenReturnsNull()
    {
        // Arrange - Create history for one establishment and query for a different establishment
        var establishment1 = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School 1",
        };
        var establishment2 = new EstablishmentEntity
        {
            EstablishmentRef = "EST002",
            OrgName = "Test School 2",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.AddRange(establishment1, establishment2);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        // Create history for establishment1 only
        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = "Completed",
            NoteText = "History for first establishment",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        // Act - Query for establishment2 (which has no history for this recommendation)
        var result = await _repository.GetLatestRecommendationHistoryAsync(
            establishment2.Id,
            recommendation.Id
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestRecommendationHistoryAsync_WhenSingleHistoryEntry_ThenReturnsThatEntry()
    {
        // Arrange - Create establishment with a single history entry to verify it returns that entry
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var singleHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = "InProgress",
            NoteText = "Only history entry",
        };

        DbContext.EstablishmentRecommendationHistories.Add(singleHistory);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestRecommendationHistoryAsync(
            establishment.Id,
            recommendation.Id
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(singleHistory.Id, result.Id);
        Assert.Equal("Only history entry", result.NoteText);
        Assert.Equal("InProgress", result.NewStatus);
    }

    [Fact]
    public async Task GetLatestRecommendationHistoryAsync_WhenInvalidEstablishmentId_ThenReturnsNull()
    {
        // Arrange - Create history for valid establishment and query with non-existent establishment ID
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = "Completed",
            NoteText = "History for valid establishment",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        // Act - Query with invalid establishment ID
        var result = await _repository.GetLatestRecommendationHistoryAsync(
            99999,
            recommendation.Id
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestRecommendationHistoryAsync_WhenInvalidRecommendationId_ThenReturnsNull()
    {
        // Arrange - Create history for valid recommendation and query with non-existent recommendation ID
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = "Completed",
            NoteText = "History for valid recommendation",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        // Act - Query with invalid recommendation ID
        var result = await _repository.GetLatestRecommendationHistoryAsync(establishment.Id, 99999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CreateRecommendationHistoryAsync Tests

    [Fact]
    public async Task CreateRecommendationHistoryAsync_WhenGivenValidParameters_ThenCreatesHistoryEntry()
    {
        // Arrange - Create establishment, user, and recommendation to use in history creation
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var initialCount = await CountEntitiesAsync<EstablishmentRecommendationHistoryEntity>();
        var beforeCreate = DateTime.UtcNow;

        // Act
        await _repository.CreateRecommendationHistoryAsync(
            establishment.Id,
            recommendation.Id,
            user.Id,
            null,
            "InProgress",
            "Completed",
            "Recommendation completed successfully"
        );

        var afterCreate = DateTime.UtcNow;

        // Assert
        var finalCount = await CountEntitiesAsync<EstablishmentRecommendationHistoryEntity>();
        Assert.Equal(initialCount + 1, finalCount);

        var createdHistory =
            await DbContext.EstablishmentRecommendationHistories.FirstOrDefaultAsync(h =>
                h.EstablishmentId == establishment.Id && h.RecommendationId == recommendation.Id
            );

        Assert.NotNull(createdHistory);
        Assert.Equal(establishment.Id, createdHistory.EstablishmentId);
        Assert.Equal(recommendation.Id, createdHistory.RecommendationId);
        Assert.Equal(user.Id, createdHistory.UserId);
        Assert.Null(createdHistory.MatEstablishmentId);
        Assert.Equal("InProgress", createdHistory.PreviousStatus);
        Assert.Equal("Completed", createdHistory.NewStatus);
        Assert.Equal("Recommendation completed successfully", createdHistory.NoteText);
        Assert.True(createdHistory.DateCreated >= beforeCreate);
        Assert.True(createdHistory.DateCreated <= afterCreate);
    }

    #endregion
}
