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
        // Arrange - Setup service to test delegation to workflow
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

        _recommendationWorkflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId)
            .Returns(expectedResult);

        var service = CreateServiceUnderTest();

        // Act
        var result = await service.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        Assert.Equal(expectedResult, result);
        await _recommendationWorkflow.Received(1).GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenWorkflowReturnsEmpty_ThenReturnsEmpty()
    {
        // Arrange - Setup workflow to return empty dictionary
        var establishmentId = 456;
        var recommendationContentfulReferences = new[] { "non-existent" };
        var emptyResult = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>();

        _recommendationWorkflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId)
            .Returns(emptyResult);

        var service = CreateServiceUnderTest();

        // Act
        var result = await service.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);

        // Assert
        Assert.Empty(result);
        await _recommendationWorkflow.Received(1).GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenEmptyReferences_ThenPassesToWorkflow()
    {
        // Arrange - Setup service with empty recommendation references
        var establishmentId = 789;
        var emptyReferences = new string[0];
        var emptyResult = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>();

        _recommendationWorkflow.GetLatestRecommendationStatusesByRecommendationIdAsync(emptyReferences, establishmentId)
            .Returns(emptyResult);

        var service = CreateServiceUnderTest();

        // Act
        var result = await service.GetLatestRecommendationStatusesByRecommendationIdAsync(emptyReferences, establishmentId);

        // Assert
        Assert.Empty(result);
        await _recommendationWorkflow.Received(1).GetLatestRecommendationStatusesByRecommendationIdAsync(emptyReferences, establishmentId);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenWorkflowThrows_ThenPropagatesException()
    {
        // Arrange - Setup workflow to throw exception to test error propagation
        var establishmentId = 999;
        var recommendationContentfulReferences = new[] { "rec-error" };
        var expectedException = new InvalidOperationException("Test exception from workflow");

        _recommendationWorkflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId)
            .ThrowsAsync(expectedException);

        var service = CreateServiceUnderTest();

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId)
        );

        Assert.Equal(expectedException.Message, actualException.Message);
        await _recommendationWorkflow.Received(1).GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);
    }

    [Fact]
    public async Task GetLatestRecommendationStatusesByRecommendationIdAsync_WhenSingleRecommendation_ThenReturnsSingleResult()
    {
        // Arrange - Setup service with single recommendation to test simple delegation
        var establishmentId = 111;
        var singleReference = new[] { "rec-single" };
        var singleResult = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            ["rec-single"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = establishmentId,
                RecommendationId = 99,
                UserId = 1,
                NewStatus = "Reviewed",
                DateCreated = DateTime.UtcNow
            }
        };

        _recommendationWorkflow.GetLatestRecommendationStatusesByRecommendationIdAsync(singleReference, establishmentId)
            .Returns(singleResult);

        var service = CreateServiceUnderTest();

        // Act
        var result = await service.GetLatestRecommendationStatusesByRecommendationIdAsync(singleReference, establishmentId);

        // Assert
        Assert.Single(result);
        Assert.Equal("rec-single", result.Keys.First());
        Assert.Equal("Reviewed", result["rec-single"].NewStatus);
        await _recommendationWorkflow.Received(1).GetLatestRecommendationStatusesByRecommendationIdAsync(singleReference, establishmentId);
    }
}
