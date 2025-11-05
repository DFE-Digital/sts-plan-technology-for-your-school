using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class RecommendationServiceTests
{
    private readonly IRecommendationWorkflow _recommendationWorkflow = Substitute.For<IRecommendationWorkflow>();

    private RecommendationService CreateServiceUnderTest() => new(_recommendationWorkflow);

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenCalled_ThenDelegatesToWorkflow()
    {
        // Arrange - Multiple recommendations with different statuses for an establishment
        var establishmentId = 123;
        var recommendationContentfulReferences = new[] { "rec-001", "rec-002" };
        var expectedResult = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            ["rec-001"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = establishmentId,
                RecommendationId = 1,
                UserId = 1,
                NewStatus = "Completed",
                DateCreated = DateTime.UtcNow.AddDays(-1)
            },
            ["rec-002"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = establishmentId,
                RecommendationId = 2,
                UserId = 1,
                NewStatus = "InProgress",
                DateCreated = DateTime.UtcNow.AddDays(-2)
            }
        };

        _recommendationWorkflow.GetLatestRecommendationStatusesAsync(establishmentId)
            .Returns(expectedResult);

        var service = CreateServiceUnderTest();

        // Act
        var result = await service.GetLatestRecommendationStatusesAsync(establishmentId);

        // Assert
        Assert.Equal(expectedResult, result);
        await _recommendationWorkflow.Received(1).GetLatestRecommendationStatusesAsync(establishmentId);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenWorkflowReturnsEmpty_ThenReturnsEmpty()
    {
        // Arrange - Establishment has no recommendation history
        var establishmentId = 456;
        var emptyResult = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>();

        _recommendationWorkflow.GetLatestRecommendationStatusesAsync(establishmentId)
            .Returns(emptyResult);

        var service = CreateServiceUnderTest();

        // Act
        var result = await service.GetLatestRecommendationStatusesAsync(establishmentId);

        // Assert
        Assert.Empty(result);
        await _recommendationWorkflow.Received(1).GetLatestRecommendationStatusesAsync(establishmentId);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenWorkflowThrows_ThenPropagatesException()
    {
        // Arrange - Workflow encounters error during recommendation lookup
        var establishmentId = 999;
        var expectedException = new InvalidOperationException("Test exception from workflow");

        _recommendationWorkflow.GetLatestRecommendationStatusesAsync(establishmentId)
            .ThrowsAsync(expectedException);

        var service = CreateServiceUnderTest();

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetLatestRecommendationStatusesAsync(establishmentId)
        );

        Assert.Equal(expectedException.Message, actualException.Message);
        await _recommendationWorkflow.Received(1).GetLatestRecommendationStatusesAsync(establishmentId);
    }

    [Fact]
    public async Task GetCurrentRecommendationStatusAsync_WhenCalled_ThenDelegatesToWorkflow()
    {
        // Arrange - Current status lookup for a specific recommendation
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;
        var expectedResult = new SqlEstablishmentRecommendationHistoryDto
        {
            EstablishmentId = establishmentId,
            RecommendationId = 1,
            UserId = 1,
            NewStatus = "Completed",
            DateCreated = DateTime.UtcNow
        };

        _recommendationWorkflow.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId)
            .Returns(expectedResult);

        var service = CreateServiceUnderTest();

        // Act
        var result = await service.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);

        // Assert
        Assert.Equal(expectedResult, result);
        await _recommendationWorkflow.Received(1).GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);
    }

    [Fact]
    public async Task GetCurrentRecommendationStatusAsync_WhenWorkflowReturnsNull_ThenReturnsNull()
    {
        // Arrange - Recommendation has no status history
        var recommendationContentfulReference = "non-existent";
        var establishmentId = 456;

        _recommendationWorkflow.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId)
            .Returns((SqlEstablishmentRecommendationHistoryDto?)null);

        var service = CreateServiceUnderTest();

        // Act
        var result = await service.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);

        // Assert
        Assert.Null(result);
        await _recommendationWorkflow.Received(1).GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);
    }

    [Fact]
    public async Task GetCurrentRecommendationStatusAsync_WhenWorkflowThrows_ThenPropagatesException()
    {
        // Arrange - Workflow encounters error during status lookup
        var recommendationContentfulReference = "rec-error";
        var establishmentId = 789;
        var expectedException = new InvalidOperationException("Test exception from workflow");

        _recommendationWorkflow.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId)
            .ThrowsAsync(expectedException);

        var service = CreateServiceUnderTest();

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId)
        );

        Assert.Equal(expectedException.Message, actualException.Message);
        await _recommendationWorkflow.Received(1).GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenCalledWithAllParameters_ThenDelegatesToWorkflow()
    {
        // Arrange - Status update with all optional parameters (note and MAT establishment ID)
        var recommendationContentfulReference = "rec-001";
        var establishmentId = 123;
        var userId = 456;
        var newStatus = "Completed";
        var noteText = "Work completed successfully";
        var matEstablishmentId = 789;

        var service = CreateServiceUnderTest();

        // Act
        await service.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            noteText,
            matEstablishmentId
        );

        // Assert
        await _recommendationWorkflow.Received(1).UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            noteText,
            matEstablishmentId
        );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenCalledWithOptionalParametersNull_ThenDelegatesToWorkflow()
    {
        // Arrange - Status update without optional parameters
        var recommendationContentfulReference = "rec-002";
        var establishmentId = 987;
        var userId = 654;
        var newStatus = "InProgress";

        var service = CreateServiceUnderTest();

        // Act
        await service.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus
        );

        // Assert - Confirms optional parameters are passed as null to workflow
        await _recommendationWorkflow.Received(1).UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            null,
            null
        );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenWorkflowThrows_ThenPropagatesException()
    {
        // Arrange - Workflow fails to update non-existent recommendation
        var recommendationContentfulReference = "rec-error";
        var establishmentId = 111;
        var userId = 222;
        var newStatus = "Failed";
        var expectedException = new InvalidOperationException("Recommendation not found");

        _recommendationWorkflow.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            null,
            null
        ).ThrowsAsync(expectedException);

        var service = CreateServiceUnderTest();

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateRecommendationStatusAsync(recommendationContentfulReference, establishmentId, userId, newStatus)
        );

        Assert.Equal(expectedException.Message, actualException.Message);
        await _recommendationWorkflow.Received(1).UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            null,
            null
        );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenCalledWithEmptyNoteText_ThenPassesEmptyStringToWorkflow()
    {
        // Arrange - Status update with empty note text
        var recommendationContentfulReference = "rec-003";
        var establishmentId = 333;
        var userId = 444;
        var newStatus = "Reviewed";
        var emptyNoteText = "";

        var service = CreateServiceUnderTest();

        // Act
        await service.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            emptyNoteText
        );

        // Assert - Confirms empty string is passed to workflow (not null)
        await _recommendationWorkflow.Received(1).UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            emptyNoteText,
            null
        );
    }
}
