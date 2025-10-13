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

        // Assert - Verify it returns the latest status with correct properties
        Assert.Single(result);
        Assert.True(result.ContainsKey("rec-001"));
        Assert.Equal("Reviewed", result["rec-001"].NewStatus);
        Assert.Equal(establishmentId, result["rec-001"].EstablishmentId);
        Assert.Equal(1, result["rec-001"].RecommendationId);
    }

    [Fact]
    public async Task GetCurrentRecommendationStatusAsync_WhenRecommendationExists_ThenReturnsCorrectlyMappedDto()
    {
        // Arrange - Setup recommendation and history to test correct DTO mapping
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "Test Recommendation", QuestionId = 1 }
        };

        var historyEntity = new EstablishmentRecommendationHistoryEntity
        {
            Id = 5,
            EstablishmentId = establishmentId,
            RecommendationId = 1,
            UserId = 42,
            NewStatus = "Completed",
            PreviousStatus = "InProgress",
            NoteText = "Work completed",
            DateCreated = DateTime.UtcNow
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(
            Arg.Is<IEnumerable<string>>(refs => refs.Contains(recommendationContentfulReference)))
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetLatestRecommendationHistoryAsync(establishmentId, 1)
            .Returns(historyEntity);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);

        // Assert - Verify all DTO properties are correctly mapped
        Assert.NotNull(result);
        Assert.Equal(establishmentId, result.EstablishmentId);
        Assert.Equal(1, result.RecommendationId);
        Assert.Equal(42, result.UserId);
        Assert.Equal("Completed", result.NewStatus);
        Assert.Equal("InProgress", result.PreviousStatus);
        Assert.Equal("Work completed", result.NoteText);
        Assert.Equal(historyEntity.DateCreated, result.DateCreated);
    }

    [Fact]
    public async Task GetCurrentRecommendationStatusAsync_WhenRecommendationNotFound_ThenReturnsNull()
    {
        // Arrange - Setup scenario where recommendation doesn't exist
        var recommendationContentfulReference = "non-existent";
        var establishmentId = 123;

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(new[] { recommendationContentfulReference })
            .Returns(new RecommendationEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentRecommendationStatusAsync_WhenNoHistoryExists_ThenReturnsNull()
    {
        // Arrange - Setup recommendation but no history entries
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "Test Recommendation", QuestionId = 1 }
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(new[] { recommendationContentfulReference })
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetLatestRecommendationHistoryAsync(establishmentId, 1)
            .Returns((EstablishmentRecommendationHistoryEntity?)null);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenRecommendationExists_ThenCreatesHistoryWithCorrectData()
    {
        // Arrange - Setup recommendation and current status to test status transition logic
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;
        var userId = 456;
        var newStatus = "Completed";
        var noteText = "Work finished successfully";
        var matEstablishmentId = 789;

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "Test Recommendation", QuestionId = 1 }
        };

        var currentHistoryEntity = new EstablishmentRecommendationHistoryEntity
        {
            Id = 1,
            EstablishmentId = establishmentId,
            RecommendationId = 1,
            UserId = 1,
            NewStatus = "InProgress",
            DateCreated = DateTime.UtcNow.AddDays(-1)
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(
            Arg.Is<IEnumerable<string>>(refs => refs.Contains(recommendationContentfulReference)))
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetLatestRecommendationHistoryAsync(establishmentId, 1)
            .Returns(currentHistoryEntity);

        var workflow = CreateServiceUnderTest();

        // Act
        await workflow.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            noteText,
            matEstablishmentId
        );

        // Assert - Verify correct status transition: current status becomes previous status
        await _establishmentRecommendationHistoryRepository.Received(1).CreateRecommendationHistoryAsync(
            establishmentId,
            1,
            userId,
            matEstablishmentId,
            "InProgress", // Should use current status as previous
            newStatus,
            noteText
        );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenNoCurrentHistory_ThenCreatesInitialHistoryEntry()
    {
        // Arrange - Setup recommendation with no existing history to test initial entry creation
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;
        var userId = 456;
        var newStatus = "InProgress";

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "Test Recommendation", QuestionId = 1 }
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(
            Arg.Is<IEnumerable<string>>(refs => refs.Contains(recommendationContentfulReference)))
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetLatestRecommendationHistoryAsync(establishmentId, 1)
            .Returns((EstablishmentRecommendationHistoryEntity?)null);

        var workflow = CreateServiceUnderTest();

        // Act
        await workflow.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus
        );

        // Assert - Verify initial history entry has null previous status and converts null noteText
        await _establishmentRecommendationHistoryRepository.Received(1).CreateRecommendationHistoryAsync(
            establishmentId,
            1,
            userId,
            null,
            null, // Should be null for initial entry
            newStatus,
            string.Empty // Should convert null noteText to empty string
        );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenRecommendationNotFound_ThenThrowsInvalidOperationException()
    {
        // Arrange - Setup scenario where recommendation doesn't exist to test error handling
        var recommendationContentfulReference = "non-existent";
        var establishmentId = 123;
        var userId = 456;
        var newStatus = "Completed";

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(new[] { recommendationContentfulReference })
            .Returns(new RecommendationEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => workflow.UpdateRecommendationStatusAsync(recommendationContentfulReference, establishmentId, userId, newStatus)
        );

        Assert.Equal($"Recommendation with ContentfulRef '{recommendationContentfulReference}' not found", exception.Message);
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenNoteTextIsNull_ThenPassesEmptyStringToRepository()
    {
        // Arrange - Setup to test null noteText handling business rule
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;
        var userId = 456;
        var newStatus = "Reviewed";

        var recommendations = new[]
        {
            new RecommendationEntity { Id = 1, ContentfulRef = "rec-001", RecommendationText = "Test Recommendation", QuestionId = 1 }
        };

        _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(
            Arg.Is<IEnumerable<string>>(refs => refs.Contains(recommendationContentfulReference)))
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository.GetLatestRecommendationHistoryAsync(establishmentId, 1)
            .Returns((EstablishmentRecommendationHistoryEntity?)null);

        var workflow = CreateServiceUnderTest();

        // Act
        await workflow.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            null // Explicitly null noteText
        );

        // Assert - Verify that a null noteText is converted to empty string (database has non-nulllable column)
        await _establishmentRecommendationHistoryRepository.Received(1).CreateRecommendationHistoryAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            string.Empty // Confirms null noteText becomes empty string
        );
    }
}
