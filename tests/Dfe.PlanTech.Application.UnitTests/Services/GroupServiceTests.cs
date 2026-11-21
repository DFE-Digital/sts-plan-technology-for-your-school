using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class GroupServiceTests
{
    private readonly IGroupWorkflow _groupWorkflow = Substitute.For<IGroupWorkflow>();

    private GroupService CreateSystemUnderTest() => new(_groupWorkflow);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<GroupService>(_groupWorkflow);
    }

    [Fact]
    public async Task GetGroupCompletedSubmissionsBySections_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        int[] establishmentIds = [701, 702];
        var expectedSubmissions = new List<SqlSubmissionDto>();
        _groupWorkflow
            .GetGroupCompletedSubmissions(Arg.Is<int[]>(ids => ids.SequenceEqual(establishmentIds)))
            .Returns(expectedSubmissions);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetGroupCompletedSubmissionsBySections(establishmentIds);

        // Assert
        Assert.Same(expectedSubmissions, result);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        var groupSchools = new List<SelectedEstablishmentDataModel>
        {
            new SelectedEstablishmentDataModelBuilder().WithEstablishmentId(701).Build(),
        };
        var expectedInformation = new List<SubmissionInformationModel>
        {
            new() { EstablishmentId = 701, Status = SubmissionStatus.InProgress },
        };
        _groupWorkflow
            .GetGroupSubmissionInformationForSection(groupSchools, "section-id")
            .Returns(expectedInformation);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetGroupSubmissionInformationForSection(
            groupSchools,
            "section-id"
        );

        // Assert
        Assert.Same(expectedInformation, result);
    }

    [Fact]
    public async Task GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRef_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        var expectedHistories =
            new List<(
                EstablishmentEntity establishment,
                EstablishmentRecommendationHistoryEntity? recommendationHistory
            )>();
        _groupWorkflow
            .GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRefAsync(
                301,
                "recommendation-ref"
            )
            .Returns(expectedHistories);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result =
            await systemUnderTest.GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRef(
                301,
                "recommendation-ref"
            );

        // Assert
        Assert.Same(expectedHistories, result);
    }
}
