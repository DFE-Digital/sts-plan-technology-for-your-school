using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class RecommendationWorkflowTests
{
    private readonly IEstablishmentRecommendationHistoryRepository _establishmentRecommendationHistoryRepository =
        Substitute.For<IEstablishmentRecommendationHistoryRepository>();
    private readonly IRecommendationRepository _recommendationRepository =
        Substitute.For<IRecommendationRepository>();
    private readonly IStoredProcedureRepository _storedProcedureRepository =
        Substitute.For<IStoredProcedureRepository>();

    private RecommendationWorkflow CreateServiceUnderTest() =>
        new(
            _establishmentRecommendationHistoryRepository,
            _recommendationRepository,
            _storedProcedureRepository
        );

    private static EstablishmentRecommendationHistoryEntity MakeHistoryEntity(
        int id,
        int dateCreatedDelta,
        int establishmentId,
        int? recommendationId,
        RecommendationEntity? recommendation,
        int userId,
        RecommendationStatus newStatus
    )
    {
        if (recommendationId is null && recommendation is null)
        {
            throw new InvalidDataException(
                $"At least one of {nameof(recommendationId)} and {nameof(recommendation)} must have a value"
            );
        }

        return new EstablishmentRecommendationHistoryEntity
        {
            Id = id,
            DateCreated = DateTime.UtcNow.AddDays(dateCreatedDelta),
            EstablishmentId = establishmentId,
            RecommendationId = recommendationId ?? recommendation!.Id,
            Recommendation = recommendation!,
            UserId = userId,
            NewStatus = newStatus.ToString(),
        };
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesAsync_WhenValidInput_ThenReturnsCorrectDictionary()
    {
        // Arrange - Setup recommendations and history data to test the grouping and ordering logic
        var establishmentId = 123;
        var recommendations = new[]
        {
            new RecommendationEntity
            {
                Id = 1,
                ContentfulRef = "rec-001",
                RecommendationText = "First Recommendation",
                QuestionId = 1,
            },
            new RecommendationEntity
            {
                Id = 2,
                ContentfulRef = "rec-002",
                RecommendationText = "Second Recommendation",
                QuestionId = 1,
            },
            new RecommendationEntity
            {
                Id = 3,
                ContentfulRef = "rec-003",
                RecommendationText = "Third Recommendation",
                QuestionId = 1,
            },
        };

        var historyEntities = new[]
        {
            // Multiple entries for rec-001 (ID 1) - should return the latest one
            MakeHistoryEntity(
                1,
                -2,
                establishmentId,
                null,
                recommendations[0],
                1,
                RecommendationStatus.InProgress
            ),
            MakeHistoryEntity(
                2,
                -1,
                establishmentId,
                null,
                recommendations[0],
                1,
                RecommendationStatus.Complete
            ),
            // Single entry for rec-002 (ID 2)
            MakeHistoryEntity(
                3,
                -3,
                establishmentId,
                null,
                recommendations[1],
                1,
                RecommendationStatus.InProgress
            ),
            // Multiple entries for rec-003 (ID 3) - should return the latest one
            MakeHistoryEntity(
                4,
                -4,
                establishmentId,
                null,
                recommendations[2],
                1,
                RecommendationStatus.NotStarted
            ),
            MakeHistoryEntity(
                5,
                -2,
                establishmentId,
                null,
                recommendations[2],
                1,
                RecommendationStatus.InProgress
            ),
            MakeHistoryEntity(
                6,
                0,
                establishmentId,
                null,
                recommendations[2],
                1,
                RecommendationStatus.Complete
            ),
        };

        _establishmentRecommendationHistoryRepository
            .GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(historyEntities);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(
            establishmentId
        );

        // Assert
        Assert.Equal(3, result.Count);

        // Verify rec-001 returns the latest status (Completed from ID 2)
        Assert.True(result.ContainsKey("rec-001"));
        Assert.Equal("Complete", result["rec-001"].NewStatus);

        // Verify rec-002 returns the single status (Reviewed from ID 3)
        Assert.True(result.ContainsKey("rec-002"));
        Assert.Equal("In progress", result["rec-002"].NewStatus);

        // Verify rec-003 returns the latest status (Reviewed from ID 6)
        Assert.True(result.ContainsKey("rec-003"));
        Assert.Equal("Complete", result["rec-003"].NewStatus);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesAsync_WhenNoRecommendationsFound_ThenReturnsEmptyDictionary()
    {
        // Arrange
        var establishmentId = 123;

        _establishmentRecommendationHistoryRepository
            .GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(new EstablishmentRecommendationHistoryEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(
            establishmentId
        );

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesAsync_WhenSingleRecommendationMultipleHistory_ThenReturnsLatestByDateCreated()
    {
        // Arrange - Setup single recommendation with multiple history entries to test ordering by DateCreated
        var establishmentId = 123;

        var recommendation = new RecommendationEntity
        {
            Id = 1,
            ContentfulRef = "rec-001",
            RecommendationText = "Test Recommendation",
            QuestionId = 1,
        };

        var historyEntities = new[]
        {
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 1,
                EstablishmentId = establishmentId,
                RecommendationId = 1,
                UserId = 1,
                Recommendation = recommendation,
                NewStatus = RecommendationStatus.NotStarted.ToString(),
                DateCreated = DateTime.UtcNow.AddDays(-5),
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 2,
                EstablishmentId = establishmentId,
                RecommendationId = 1,
                Recommendation = recommendation,
                UserId = 1,
                NewStatus = RecommendationStatus.InProgress.ToString(),
                DateCreated = DateTime.UtcNow.AddDays(-2),
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 3,
                EstablishmentId = establishmentId,
                RecommendationId = 1,
                Recommendation = recommendation,
                UserId = 1,
                NewStatus = RecommendationStatus.Complete.ToString(),
                DateCreated = DateTime.UtcNow, // Most recent
            },
        };

        _establishmentRecommendationHistoryRepository
            .GetRecommendationHistoryByEstablishmentIdAsync(establishmentId)
            .Returns(historyEntities);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(
            establishmentId
        );

        // Assert - Verify it returns the latest status with correct properties
        Assert.Single(result);
        Assert.True(result.ContainsKey("rec-001"));
        Assert.Equal("Complete", result["rec-001"].NewStatus);
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
            new RecommendationEntity
            {
                Id = 1,
                ContentfulRef = "rec-001",
                RecommendationText = "Test Recommendation",
                QuestionId = 1,
            },
        };

        var historyEntity = new EstablishmentRecommendationHistoryEntity
        {
            Id = 5,
            EstablishmentId = establishmentId,
            RecommendationId = 1,
            UserId = 42,
            NewStatus = RecommendationStatus.Complete.ToString(),
            PreviousStatus = RecommendationStatus.InProgress.ToString(),
            NoteText = "Work completed",
            DateCreated = DateTime.UtcNow,
        };

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                Arg.Is<IEnumerable<string>>(refs =>
                    refs.Contains(recommendationContentfulReference)
                )
            )
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository
            .GetLatestRecommendationHistoryAsync(establishmentId, 1)
            .Returns(historyEntity);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetCurrentRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId
        );

        // Assert - Verify all DTO properties are correctly mapped
        Assert.NotNull(result);
        Assert.Equal(establishmentId, result.EstablishmentId);
        Assert.Equal(1, result.RecommendationId);
        Assert.Equal(42, result.UserId);
        Assert.Equal("Complete", result.NewStatus);
        Assert.Equal("In progress", result.PreviousStatus);
        Assert.Equal("Work completed", result.NoteText);
        Assert.Equal(historyEntity.DateCreated, result.DateCreated);
    }

    [Fact]
    public async Task GetCurrentRecommendationStatusAsync_WhenRecommendationNotFound_ThenReturnsNull()
    {
        // Arrange - Setup scenario where recommendation doesn't exist
        var recommendationContentfulReference = "non-existent";
        var establishmentId = 123;

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                new[] { recommendationContentfulReference }
            )
            .Returns(new RecommendationEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetCurrentRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId
        );

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
            new RecommendationEntity
            {
                Id = 1,
                ContentfulRef = "rec-001",
                RecommendationText = "Test Recommendation",
                QuestionId = 1,
            },
        };

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                new[] { recommendationContentfulReference }
            )
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository
            .GetLatestRecommendationHistoryAsync(establishmentId, 1)
            .Returns((EstablishmentRecommendationHistoryEntity?)null);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetCurrentRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecommendationHistoryAsync_WhenNoRecommendationFound_ThenReturnsEmpty()
    {
        // Arrange - Setup recommendation but no history entries
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                new[] { recommendationContentfulReference }
            )
            .Returns([]);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetRecommendationHistoryAsync(
            recommendationContentfulReference,
            establishmentId
        );

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationHistoryAsync_WhenRecommendationExists_ThenReturnsHistory()
    {
        // Arrange - Setup recommendation but no history entries
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;

        var recommendation = new RecommendationEntity
        {
            Id = 1,
            ContentfulRef = recommendationContentfulReference,
            RecommendationText = "Test Recommendation",
            QuestionId = 1,
        };

        var historyEntities = new[]
        {
            MakeHistoryEntity(
                1,
                -4,
                establishmentId,
                null,
                recommendation,
                1,
                RecommendationStatus.NotStarted
            ),
            MakeHistoryEntity(
                2,
                -2,
                establishmentId,
                null,
                recommendation,
                1,
                RecommendationStatus.InProgress
            ),
            MakeHistoryEntity(
                3,
                0,
                establishmentId,
                null,
                recommendation,
                1,
                RecommendationStatus.Complete
            ),
        };

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                Arg.Is<IEnumerable<string>>(refs =>
                    refs.Contains(recommendationContentfulReference)
                )
            )
            .Returns([recommendation]);
        _establishmentRecommendationHistoryRepository
            .GetRecommendationHistoryByEstablishmentIdAndRecommendationIdAsync(establishmentId, 1)
            .Returns(historyEntities);

        var workflow = CreateServiceUnderTest();

        // Act
        var result = await workflow.GetRecommendationHistoryAsync(
            recommendationContentfulReference,
            establishmentId
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
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
            new RecommendationEntity
            {
                Id = 1,
                ContentfulRef = "rec-001",
                RecommendationText = "Test Recommendation",
                QuestionId = 1,
            },
        };

        var currentHistoryEntity = new EstablishmentRecommendationHistoryEntity
        {
            Id = 1,
            EstablishmentId = establishmentId,
            RecommendationId = 1,
            UserId = 1,
            NewStatus = "InProgress",
            DateCreated = DateTime.UtcNow.AddDays(-1),
        };

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                Arg.Is<IEnumerable<string>>(refs =>
                    refs.Contains(recommendationContentfulReference)
                )
            )
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository
            .GetLatestRecommendationHistoryAsync(establishmentId, 1)
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
        await _establishmentRecommendationHistoryRepository
            .Received(1)
            .CreateRecommendationHistoryAsync(
                establishmentId,
                1,
                userId,
                matEstablishmentId,
                "In progress", // Should use current status as previous
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
            new RecommendationEntity
            {
                Id = 1,
                ContentfulRef = "rec-001",
                RecommendationText = "Test Recommendation",
                QuestionId = 1,
            },
        };

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                Arg.Is<IEnumerable<string>>(refs =>
                    refs.Contains(recommendationContentfulReference)
                )
            )
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository
            .GetLatestRecommendationHistoryAsync(establishmentId, 1)
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
        await _establishmentRecommendationHistoryRepository
            .Received(1)
            .CreateRecommendationHistoryAsync(
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

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                new[] { recommendationContentfulReference }
            )
            .Returns(new RecommendationEntity[0]);

        var workflow = CreateServiceUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            workflow.UpdateRecommendationStatusAsync(
                recommendationContentfulReference,
                establishmentId,
                userId,
                newStatus
            )
        );

        Assert.Equal(
            $"Recommendation with ContentfulRef '{recommendationContentfulReference}' not found",
            exception.Message
        );
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
            new RecommendationEntity
            {
                Id = 1,
                ContentfulRef = "rec-001",
                RecommendationText = "Test Recommendation",
                QuestionId = 1,
            },
        };

        _recommendationRepository
            .GetRecommendationsByContentfulReferencesAsync(
                Arg.Is<IEnumerable<string>>(refs =>
                    refs.Contains(recommendationContentfulReference)
                )
            )
            .Returns(recommendations);
        _establishmentRecommendationHistoryRepository
            .GetLatestRecommendationHistoryAsync(establishmentId, 1)
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
        await _establishmentRecommendationHistoryRepository
            .Received(1)
            .CreateRecommendationHistoryAsync(
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
