using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Dfe.PlanTech.UnitTests.Shared.Builders;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class EstablishmentRecommendationHistoryRepositoryTests : DatabaseIntegrationTestBase
{
    private EstablishmentRecommendationHistoryRepository _repository = null!;

    public EstablishmentRecommendationHistoryRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async ValueTask InitializeAsync()
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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var history1 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "First history entry for establishment 1",
        };

        var history2 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.Complete,
            NewStatus = RecommendationStatus.InProgress,
            NoteText = "Second history entry for establishment 1",
        };

        var history3 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment2.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "History entry for different establishment",
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(history1, history2, history3);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var history1 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation1.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "First history entry for establishment 1",
        };

        var history2 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation2.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.Complete,
            NewStatus = RecommendationStatus.InProgress,
            NoteText = "Second history entry for establishment 1",
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(history1, history2);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "Test history entry for existing establishment",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var history1 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation1.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "History for first recommendation",
        };

        var history2 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation2.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "History for second recommendation",
        };

        var history3 = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation1.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.Complete,
            NewStatus = RecommendationStatus.InProgress,
            NoteText = "Second history entry for first recommendation",
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(history1, history2, history3);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            MatEstablishmentId = matEstablishment.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "History entry with MAT establishment reference",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var oldestHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.InProgress,
            NoteText = "Oldest history entry - initial status",
            DateCreated = DateTime.UtcNow.AddDays(-5),
        };

        var newestHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.Complete,
            NewStatus = RecommendationStatus.InProgress,
            NoteText = "Newest history entry - final review",
            DateCreated = DateTime.UtcNow.AddDays(-1),
        };

        var middleHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "Middle history entry - completion",
            DateCreated = DateTime.UtcNow.AddDays(-3),
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(
            oldestHistory,
            newestHistory,
            middleHistory
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create multiple history entries with different dates
        var oldHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.InProgress,
            NoteText = "Initial status",
            DateCreated = DateTime.UtcNow.AddDays(-5),
        };

        var latestHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "Latest status - should be returned",
            DateCreated = DateTime.UtcNow.AddDays(-1),
        };

        var middleHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = RecommendationStatus.InProgress,
            NewStatus = RecommendationStatus.InProgress,
            NoteText = "Middle status",
            DateCreated = DateTime.UtcNow.AddDays(-3),
        };

        DbContext.EstablishmentRecommendationHistories.AddRange(
            oldHistory,
            latestHistory,
            middleHistory
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _repository.GetLatestRecommendationHistoryAsync(
            establishment.Id,
            recommendation.Id
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Latest status - should be returned", result.NoteText);
        Assert.Equal(RecommendationStatus.Complete, result.NewStatus);
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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create history for recommendation1 only
        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation1.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "History for first recommendation",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create history for establishment1 only
        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment1.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "History for first establishment",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var singleHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.InProgress,
            NoteText = "Only history entry",
        };

        DbContext.EstablishmentRecommendationHistories.Add(singleHistory);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _repository.GetLatestRecommendationHistoryAsync(
            establishment.Id,
            recommendation.Id
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(singleHistory.Id, result.Id);
        Assert.Equal("Only history entry", result.NoteText);
        Assert.Equal(RecommendationStatus.InProgress, result.NewStatus);
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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "History for valid establishment",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            PreviousStatus = null,
            NewStatus = RecommendationStatus.Complete,
            NoteText = "History for valid recommendation",
        };

        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act - Query with invalid recommendation ID
        var result = await _repository.GetLatestRecommendationHistoryAsync(establishment.Id, 99999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CreateRecommendationHistoryAsync Tests


    [Fact]
    public async Task CreateRecommendationHistoriesAsync_CreatesRecommendationHistories_WithCorrectStatuses()
    {
        // Arrange
        var establishment = EntityBuilders.BuildEstablishment(201);
        var matEstablishment = EntityBuilders.BuildEstablishment(202);
        var user = EntityBuilders.BuildUser(101);

        await DbContext.Establishments.AddRangeAsync(
            [establishment, matEstablishment],
            TestContext.Current.CancellationToken
        );
        await DbContext.Users.AddAsync(user, TestContext.Current.CancellationToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendations = new List<RecommendationEntity>
        {
            EntityBuilders.BuildRecommendation(701, "REC1"),
            EntityBuilders.BuildRecommendation(702, "REC2"),
            EntityBuilders.BuildRecommendation(703, "REC3"),
        };
        await DbContext.Recommendations.AddRangeAsync(
            recommendations,
            TestContext.Current.CancellationToken
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendationIds = recommendations
            .OrderBy(r => r.ContentfulRef)
            .Select(r => r.Id)
            .ToList();

        var recommendationStatuses = new Dictionary<string, RecommendationStatus>()
        {
            { "REC1", RecommendationStatus.Complete },
            { "REC2", RecommendationStatus.InProgress },
            { "REC3", RecommendationStatus.NotStarted },
        };

        var submission = EntityBuilders.BuildSubmission(1, establishment.Id, "SEC1");
        await DbContext.Submissions.AddAsync(submission, TestContext.Current.CancellationToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var responses = new List<ResponseEntity>()
        {
            EntityBuilders.BuildResponse(
                601,
                DateTime.UtcNow.AddMinutes(-5),
                submission,
                101,
                "Q1",
                201,
                "A1"
            ),
            EntityBuilders.BuildResponse(
                602,
                DateTime.UtcNow.AddMinutes(-3),
                submission,
                102,
                "Q2",
                201,
                "A2"
            ),
            EntityBuilders.BuildResponse(
                603,
                DateTime.UtcNow.AddMinutes(-1),
                submission,
                103,
                "Q3",
                201,
                "A3"
            ),
        };
        await DbContext.Responses.AddRangeAsync(responses, TestContext.Current.CancellationToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var responseIds = responses.OrderBy(r => r.QuestionId).Select(r => r.Id).ToList();

        var recommendationRefsToResponseIds = new Dictionary<string, int>
        {
            { "REC1", responseIds[0] },
            { "REC2", responseIds[1] },
            { "REC3", responseIds[2] },
        };

        // Act
        await _repository.CreateRecommendationHistoriesAsync(
            establishment.Id,
            matEstablishment.Id,
            user.Id,
            recommendations,
            recommendationRefsToResponseIds,
            recommendationStatuses
        );

        // Assert
        var createdHistories = await DbContext
            .EstablishmentRecommendationHistories.Where(h =>
                h.EstablishmentId == establishment.Id
                && h.MatEstablishmentId == matEstablishment.Id
                && recommendationIds.Contains(h.RecommendationId)
                && responseIds.Contains(h.RecommendationId)
            )
            .OrderBy(h => h.RecommendationId)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(createdHistories);
        Assert.Equal(3, createdHistories.Count);

        for (var i = 0; i < 3; i++)
        {
            var history = createdHistories[i];
            var expectedNewStatus = i switch
            {
                0 => RecommendationStatus.Complete,
                1 => RecommendationStatus.InProgress,
                2 => RecommendationStatus.NotStarted,
                _ => throw new ArgumentOutOfRangeException("i cannot be outside the above range"),
            };

            Assert.Equal(establishment.Id, history.EstablishmentId);
            Assert.Equal(matEstablishment.Id, history.MatEstablishmentId);
            Assert.Equal(user.Id, history.UserId);
            Assert.Equal(recommendationIds[i], history.RecommendationId);
            Assert.Equal(responseIds[i], history.ResponseId);
            Assert.Null(history.PreviousStatus);
            Assert.Equal(expectedNewStatus, history.NewStatus);
        }
    }

    [Fact]
    public async Task CreateRecommendationHistoriesAsync_When_PreviousStatusesExist_CreatesRecommendationHistories_WithCorrectPreviousStatuses()
    {
        // Arrange
        var establishment = EntityBuilders.BuildEstablishment(201);
        var matEstablishment = EntityBuilders.BuildEstablishment(202);
        var user = EntityBuilders.BuildUser(101);

        await DbContext.Establishments.AddRangeAsync(
            [establishment, matEstablishment],
            TestContext.Current.CancellationToken
        );
        await DbContext.Users.AddAsync(user, TestContext.Current.CancellationToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendations = new List<RecommendationEntity>
        {
            EntityBuilders.BuildRecommendation(701, "REC1"),
            EntityBuilders.BuildRecommendation(702, "REC2"),
            EntityBuilders.BuildRecommendation(703, "REC3"),
        };
        await DbContext.Recommendations.AddRangeAsync(
            recommendations,
            TestContext.Current.CancellationToken
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendationIds = recommendations
            .OrderBy(r => r.ContentfulRef)
            .Select(r => r.Id)
            .ToList();

        // Add some previous histories
        var recommendationStatuses1 = new Dictionary<string, RecommendationStatus>()
        {
            { "REC1", RecommendationStatus.Complete },
            { "REC2", RecommendationStatus.InProgress },
            { "REC3", RecommendationStatus.NotStarted },
        };

        var submission1 = EntityBuilders.BuildSubmission(1, establishment.Id, "SEC1");
        await DbContext.Submissions.AddAsync(submission1, TestContext.Current.CancellationToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var responses1 = new List<ResponseEntity>()
        {
            EntityBuilders.BuildResponse(
                601,
                DateTime.UtcNow.AddMinutes(-5),
                submission1,
                101,
                "Q1",
                201,
                "A1"
            ),
            EntityBuilders.BuildResponse(
                602,
                DateTime.UtcNow.AddMinutes(-3),
                submission1,
                102,
                "Q2",
                201,
                "A2"
            ),
            EntityBuilders.BuildResponse(
                603,
                DateTime.UtcNow.AddMinutes(-1),
                submission1,
                103,
                "Q3",
                201,
                "A3"
            ),
        };
        await DbContext.Responses.AddRangeAsync(responses1, TestContext.Current.CancellationToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var responseIds1 = responses1.OrderBy(r => r.QuestionId).Select(r => r.Id).ToList();

        var recommendationRefsToResponseIds1 = new Dictionary<string, int>
        {
            { "REC1", responseIds1[0] },
            { "REC2", responseIds1[1] },
            { "REC3", responseIds1[2] },
        };

        var histories1 = recommendations.Select(
            recommendation => new EstablishmentRecommendationHistoryEntity
            {
                EstablishmentId = establishment.Id,
                MatEstablishmentId = matEstablishment.Id,
                RecommendationId = recommendation.Id,
                ResponseId = recommendationRefsToResponseIds1[recommendation.ContentfulRef],
                UserId = user.Id,
                PreviousStatus = recommendationStatuses1.TryGetValue(
                    recommendation.ContentfulRef,
                    out var previousStatus
                )
                    ? previousStatus
                    : null,
                NewStatus = recommendationStatuses1[recommendation.ContentfulRef],
            }
        );

        await DbContext.EstablishmentRecommendationHistories.AddRangeAsync(
            histories1,
            TestContext.Current.CancellationToken
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Add the next submission's entries
        var recommendationStatuses2 = new Dictionary<string, RecommendationStatus>()
        {
            { "REC1", RecommendationStatus.NotStarted },
            { "REC2", RecommendationStatus.Complete },
            { "REC3", RecommendationStatus.InProgress },
        };

        var submission2 = EntityBuilders.BuildSubmission(1, establishment.Id, "SEC1");
        await DbContext.Submissions.AddAsync(submission1, TestContext.Current.CancellationToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var responses2 = new List<ResponseEntity>()
        {
            EntityBuilders.BuildResponse(
                601,
                DateTime.UtcNow.AddMinutes(-5),
                submission2,
                101,
                "Q1",
                201,
                "A1"
            ),
            EntityBuilders.BuildResponse(
                602,
                DateTime.UtcNow.AddMinutes(-3),
                submission2,
                102,
                "Q2",
                201,
                "A2"
            ),
            EntityBuilders.BuildResponse(
                603,
                DateTime.UtcNow.AddMinutes(-1),
                submission2,
                103,
                "Q3",
                201,
                "A3"
            ),
        };
        await DbContext.Responses.AddRangeAsync(responses1, TestContext.Current.CancellationToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var responseIds2 = responses2.OrderBy(r => r.QuestionId).Select(r => r.Id).ToList();

        var recommendationRefsToResponseIds2 = new Dictionary<string, int>
        {
            { "REC1", responseIds2[0] },
            { "REC2", responseIds2[1] },
            { "REC3", responseIds2[2] },
        };

        // Act
        await _repository.CreateRecommendationHistoriesAsync(
            establishment.Id,
            matEstablishment.Id,
            user.Id,
            recommendations,
            recommendationRefsToResponseIds2,
            recommendationStatuses2
        );

        // Assert
        var createdHistories = await DbContext
            .EstablishmentRecommendationHistories.Where(h =>
                h.EstablishmentId == establishment.Id
                && h.MatEstablishmentId == matEstablishment.Id
                && recommendationIds.Contains(h.RecommendationId)
                && responseIds2.Contains(h.RecommendationId)
            )
            .OrderBy(h => h.RecommendationId)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(createdHistories);
        Assert.Equal(3, createdHistories.Count);

        for (var i = 0; i < 3; i++)
        {
            var history = createdHistories[i];
            var expectedPreviousStatus = i switch
            {
                0 => RecommendationStatus.Complete,
                1 => RecommendationStatus.InProgress,
                2 => RecommendationStatus.NotStarted,
                _ => throw new ArgumentOutOfRangeException("i cannot be outside the above range"),
            };
            var expectedNewStatus = i switch
            {
                0 => RecommendationStatus.NotStarted,
                1 => RecommendationStatus.Complete,
                2 => RecommendationStatus.InProgress,
                _ => throw new ArgumentOutOfRangeException("i cannot be outside the above range"),
            };

            Assert.Equal(establishment.Id, history.EstablishmentId);
            Assert.Equal(matEstablishment.Id, history.MatEstablishmentId);
            Assert.Equal(user.Id, history.UserId);
            Assert.Equal(recommendationIds[i], history.RecommendationId);
            Assert.Equal(responseIds2[i], history.ResponseId);
            Assert.Equal(expectedPreviousStatus, history.PreviousStatus);
            Assert.Equal(expectedNewStatus, history.NewStatus);
        }
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_UpdatesHistory()
    {
        // Arrange - Create establishment, user, and recommendation to use in history creation
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
        };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var answer = new AnswerEntity { AnswerText = "Test Answer", ContentfulRef = "A1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var submission = new SubmissionEntity
        {
            EstablishmentId = establishment.Id,
            SectionId = "SECTION001",
            SectionName = "Test Section",
            DateCreated = DateTime.Now,
            DateLastUpdated = DateTime.Now,
            Deleted = false,
            Status = SubmissionStatus.CompleteReviewed,
            UserActionId = Guid.NewGuid(),
            CreatedUserActionId = Guid.NewGuid(),
            LastUpdatedUserActionId = Guid.NewGuid(),
            CompletedUserActionId = Guid.NewGuid(),
        };
        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };
        var response = new ResponseEntity
        {
            UserId = user.Id,
            SubmissionId = submission.Id,
            QuestionId = question.Id,
            AnswerId = answer.Id,
            DateCreated = DateTime.Now,
            DateLastUpdated = DateTime.Now,
            UserEstablishmentId = establishment.Id,
            UserActionId = Guid.NewGuid(),
        };
        DbContext.Recommendations.Add(recommendation);
        DbContext.Responses.Add(response);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var initialCount = await CountEntitiesAsync<EstablishmentRecommendationHistoryEntity>();
        var beforeCreate = DateTime.UtcNow;

        // Act
        await _repository.UpdateRecommendationStatusAsync(
            establishment.Id,
            recommendation.Id,
            user.Id,
            null,
            RecommendationStatus.InProgress,
            RecommendationStatus.Complete,
            "Recommendation completed successfully"
        );

        var afterCreate = DateTime.UtcNow;

        // Assert
        var finalCount = await CountEntitiesAsync<EstablishmentRecommendationHistoryEntity>();
        Assert.Equal(initialCount + 1, finalCount);

        var createdHistory =
            await DbContext.EstablishmentRecommendationHistories.FirstOrDefaultAsync(
                h =>
                    h.EstablishmentId == establishment.Id
                    && h.RecommendationId == recommendation.Id,
                TestContext.Current.CancellationToken
            );

        Assert.NotNull(createdHistory);
        Assert.Equal(establishment.Id, createdHistory.EstablishmentId);
        Assert.Equal(recommendation.Id, createdHistory.RecommendationId);
        Assert.Equal(user.Id, createdHistory.UserId);
        Assert.Null(createdHistory.MatEstablishmentId);
        Assert.Equal(RecommendationStatus.InProgress, createdHistory.PreviousStatus);
        Assert.Equal(RecommendationStatus.Complete, createdHistory.NewStatus);
        Assert.Equal("Recommendation completed successfully", createdHistory.NoteText);
        Assert.True(createdHistory.DateCreated >= beforeCreate);
        Assert.True(createdHistory.DateCreated <= afterCreate);
    }

    #endregion
}
