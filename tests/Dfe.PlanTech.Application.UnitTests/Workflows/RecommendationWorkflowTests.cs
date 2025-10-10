using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class RecommendationWorkflowTests
{
    private readonly IEstablishmentRecommendationHistoryRepository _establishmentRecommendationHistoryRepository = Substitute.For<IEstablishmentRecommendationHistoryRepository>();
    private readonly IRecommendationRepository _recommendationRepository = Substitute.For<IRecommendationRepository>();

    private RecommendationWorkflow CreateServiceUnderTest() => new(_establishmentRecommendationHistoryRepository, _recommendationRepository);

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenValidInput_ThenReturnsCorrectDictionary()
    {
        // Arrange - Setup recommendations and history data to test the grouping and ordering logic
        var establishmentId = 123;
        var recommendationContentfulReferences = new[] { "rec-001", "rec-002", "rec-003" };

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "First Recommendation", QuestionId = 1 },
            new RecommendationEntity { Id = 2, ContentfulRef = "rec-002", RecommendationText = "Second Recommendation", QuestionId = 1 },
            new RecommendationEntity { Id = 3, ContentfulRef = "rec-003", RecommendationText = "Third Recommendation", QuestionId = 1 }
        };

        var historyEntities = new[]
        {
            // Multiple entries for rec-001 (ID 1) - should return the latest one
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 1, EstablishmentId = establishmentId, RecommendationId = 1, UserId = 1,
                NewStatus = "InProgress", DateCreated = DateTime.UtcNow.AddDays(-2)
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 2, EstablishmentId = establishmentId, RecommendationId = 1, UserId = 1,
                NewStatus = "Completed", DateCreated = DateTime.UtcNow.AddDays(-1)
            },

            // Single entry for rec-002 (ID 2)
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 3, EstablishmentId = establishmentId, RecommendationId = 2, UserId = 1,
                NewStatus = "Reviewed", DateCreated = DateTime.UtcNow.AddDays(-3)
            },

            // Multiple entries for rec-003 (ID 3) - should return the latest one
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 4, EstablishmentId = establishmentId, RecommendationId = 3, UserId = 1,
                NewStatus = "InProgress", DateCreated = DateTime.UtcNow.AddDays(-4)
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 5, EstablishmentId = establishmentId, RecommendationId = 3, UserId = 1,
                NewStatus = "Completed", DateCreated = DateTime.UtcNow.AddDays(-2)
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 6, EstablishmentId = establishmentId, RecommendationId = 3, UserId = 1,
                NewStatus = "Reviewed", DateCreated = DateTime.UtcNow
            }
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences)
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(historyEntities);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        Assert.Equal(3, result.Count);

        // Verify rec-001 returns the latest status (Completed from ID 2)
        Assert.True(result.ContainsKey("rec-001"));
        Assert.Equal("Completed", result["rec-001"].NewStatus);

        // Verify rec-002 returns the single status (Reviewed from ID 3)
        Assert.True(result.ContainsKey("rec-002"));
        Assert.Equal("Reviewed", result["rec-002"].NewStatus);

        // Verify rec-003 returns the latest status (Reviewed from ID 6)
        Assert.True(result.ContainsKey("rec-003"));
        Assert.Equal("Reviewed", result["rec-003"].NewStatus);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenNoRecommendationsFound_ThenReturnsEmptyDictionary()
    {
        // Arrange - Setup scenario where no recommendations match the provided references
        var establishmentId = 123;
        var recommendationContentfulReferences = new[] { "non-existent-rec" };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences)
            .Returns(new RecommendationEntity[0]);
        _establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(new EstablishmentRecommendationHistoryEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenNoHistoryForRecommendations_ThenReturnsEmptyDictionary()
    {
        // Arrange - Setup recommendations but no history entries for the establishment
        var establishmentId = 123;
        var recommendationContentfulReferences = new[] { "rec-001", "rec-002" };

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "First Recommendation", QuestionId = 1 },
            new RecommendationEntity { Id = 2, ContentfulRef = "rec-002", RecommendationText = "Second Recommendation", QuestionId = 1 }
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences)
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(new EstablishmentRecommendationHistoryEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenPartialHistoryMatch_ThenReturnsOnlyMatchingEntries()
    {
        // Arrange - Setup recommendations where only some have history entries
        var establishmentId = 123;
        var recommendationContentfulReferences = new[] { "rec-001", "rec-002", "rec-003" };

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "First Recommendation", QuestionId = 1 },
            new RecommendationEntity { Id = 2, ContentfulRef = "rec-002", RecommendationText = "Second Recommendation", QuestionId = 1 },
            new RecommendationEntity { Id = 3, ContentfulRef = "rec-003", RecommendationText = "Third Recommendation", QuestionId = 1 }
        };

        // Only provide history for recommendations 1 and 3, not 2
        var historyEntities = new[]
        {
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 1, EstablishmentId = establishmentId, RecommendationId = 1, UserId = 1,
                NewStatus = "Completed", DateCreated = DateTime.UtcNow.AddDays(-1)
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 2, EstablishmentId = establishmentId, RecommendationId = 3, UserId = 1,
                NewStatus = "InProgress", DateCreated = DateTime.UtcNow.AddDays(-2)
            }
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences)
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(historyEntities);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("rec-001"));
        Assert.True(result.ContainsKey("rec-003"));
        Assert.False(result.ContainsKey("rec-002")); // No history for this recommendation

        Assert.Equal("Completed", result["rec-001"].NewStatus);
        Assert.Equal("InProgress", result["rec-003"].NewStatus);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenEmptyInputReferences_ThenReturnsEmptyDictionary()
    {
        // Arrange - Setup empty recommendation references
        var establishmentId = 123;
        var recommendationContentfulReferences = new string[0];

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences)
            .Returns(new RecommendationEntity[0]);
        _establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(new EstablishmentRecommendationHistoryEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenSingleRecommendationMultipleHistory_ThenReturnsLatestByDateCreated()
    {
        // Arrange - Setup single recommendation with multiple history entries to test ordering by DateCreated
        var establishmentId = 123;
        var recommendationContentfulReferences = new[] { "rec-001" };

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "Test Recommendation", QuestionId = 1 }
        };

        var historyEntities = new[]
        {
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 1, EstablishmentId = establishmentId, RecommendationId = 1, UserId = 1,
                NewStatus = "InProgress", DateCreated = DateTime.UtcNow.AddDays(-5)
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 2, EstablishmentId = establishmentId, RecommendationId = 1, UserId = 1,
                NewStatus = "Completed", DateCreated = DateTime.UtcNow.AddDays(-2)
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 3, EstablishmentId = establishmentId, RecommendationId = 1, UserId = 1,
                NewStatus = "Reviewed", DateCreated = DateTime.UtcNow // Most recent
            }
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences)
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(historyEntities);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey("rec-001"));
        Assert.Equal("Reviewed", result["rec-001"].NewStatus); // Should be the most recent entry
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_VerifyRepositoryInteractions()
    {
        // Arrange - Setup to verify that both repositories are called with correct parameters
        var establishmentId = 456;
        var recommendationContentfulReferences = new[] { "rec-test" };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences)
            .Returns(new RecommendationEntity[0]);
        _establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(new EstablishmentRecommendationHistoryEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act
        await workflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        await _recommendationRepository.Received(1).GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences);
        await _establishmentRecommendationHistoryRepository.Received(1).GetRecommendationHistoryByEstablishmentIdAsync(establishmentId);
    }
}
