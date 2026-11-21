using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class GroupWorkflowTests
{
    private readonly IEstablishmentGroupRepository _establishmentGroupRepository =
        Substitute.For<IEstablishmentGroupRepository>();
    private readonly IEstablishmentRecommendationHistoryRepository _recommendationHistoryRepository =
        Substitute.For<IEstablishmentRecommendationHistoryRepository>();
    private readonly ISubmissionRepository _submissionRepository =
        Substitute.For<ISubmissionRepository>();

    private GroupWorkflow CreateSystemUnderTest() =>
        new(_establishmentGroupRepository, _recommendationHistoryRepository, _submissionRepository);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<GroupWorkflow>(
            _establishmentGroupRepository,
            _recommendationHistoryRepository,
            _submissionRepository
        );
    }

    [Fact]
    public async Task GetGroupCompletedSubmissions_WhenSubmissionsExist_ReturnsMappedSubmissions()
    {
        // Arrange
        int[] establishmentIds = [701, 702];
        _submissionRepository
            .GetEstablishmentsLatestCompletedSubmissionsBySectionsAsync(
                Arg.Is<IEnumerable<int>>(ids => ids.SequenceEqual(establishmentIds))
            )
            .Returns([
                new SubmissionEntityBuilder()
                    .WithId(100)
                    .WithEstablishmentId(701)
                    .WithSectionId("section-a")
                    .Build(),
                new SubmissionEntityBuilder()
                    .WithId(200)
                    .WithEstablishmentId(702)
                    .WithSectionId("section-b")
                    .Build(),
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetGroupCompletedSubmissions(establishmentIds);

        // Assert
        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(100, submission.Id);
                Assert.Equal(701, submission.EstablishmentId);
                Assert.Equal("section-a", submission.SectionId);
            },
            submission =>
            {
                Assert.Equal(200, submission.Id);
                Assert.Equal(702, submission.EstablishmentId);
                Assert.Equal("section-b", submission.SectionId);
            }
        );
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_WhenSchoolHasNoSubmission_ReturnsNotStartedEntry()
    {
        // Arrange
        var school = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(701)
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 701 })),
                "section-id"
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetGroupSubmissionInformationForSection(
            [school],
            "section-id"
        );

        // Assert
        var information = Assert.Single(result);
        Assert.Equal(701, information.EstablishmentId);
        Assert.Equal("Northfield Primary", information.EstablishmentName);
        Assert.Equal("100701", information.EstablishmentRef);
        Assert.Equal("section-id", information.SectionId);
        Assert.Equal(SubmissionStatus.NotStarted, information.Status);
        Assert.Null(information.SubmissionId);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_WhenSchoolHasSubmission_ReturnsSubmissionDetails()
    {
        // Arrange
        var school = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(701)
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Any<IEnumerable<int>>(),
                "section-id"
            )
            .Returns([
                new SubmissionEntityBuilder()
                    .WithId(100)
                    .WithEstablishmentId(701)
                    .WithStatus(SubmissionStatus.CompleteReviewed)
                    .WithDateCreated(new DateTime(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc))
                    .WithDateLastUpdated(new DateTime(2025, 2, 20, 9, 0, 0, DateTimeKind.Utc))
                    .WithDateCompleted(new DateTime(2025, 3, 25, 9, 0, 0, DateTimeKind.Utc))
                    .Build(),
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetGroupSubmissionInformationForSection(
            [school],
            "section-id"
        );

        // Assert
        var information = Assert.Single(result);
        Assert.Equal(100, information.SubmissionId);
        Assert.Equal(SubmissionStatus.CompleteReviewed, information.Status);
        Assert.Equal("15 Jan 2025", information.DateCreated);
        Assert.Equal("20 Feb 2025", information.DateLastUpdated);
        Assert.Equal("25 Mar 2025", information.DateCompleted);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_WhenSubmissionHasNoUpdateOrCompletionDate_LeavesThoseDatesNull()
    {
        // Arrange
        var school = new SelectedEstablishmentDataModelBuilder().WithEstablishmentId(701).Build();
        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Any<IEnumerable<int>>(),
                "section-id"
            )
            .Returns([new SubmissionEntityBuilder().WithId(100).WithEstablishmentId(701).Build()]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetGroupSubmissionInformationForSection(
            [school],
            "section-id"
        );

        // Assert
        var information = Assert.Single(result);
        Assert.Null(information.DateLastUpdated);
        Assert.Null(information.DateCompleted);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_WhenRepositoryReturnsNull_ReturnsNotStartedEntries()
    {
        // Arrange
        var school = new SelectedEstablishmentDataModelBuilder().WithEstablishmentId(701).Build();
        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Any<IEnumerable<int>>(),
                "section-id"
            )
            .Returns((List<SubmissionEntity>)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetGroupSubmissionInformationForSection(
            [school],
            "section-id"
        );

        // Assert
        var information = Assert.Single(result);
        Assert.Equal(SubmissionStatus.NotStarted, information.Status);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_WhenSchoolsRepeatAnEstablishmentId_QueriesDistinctIds()
    {
        // Arrange
        List<SelectedEstablishmentDataModel> schools =
        [
            new SelectedEstablishmentDataModelBuilder().WithEstablishmentId(701).Build(),
            new SelectedEstablishmentDataModelBuilder().WithEstablishmentId(702).Build(),
            new SelectedEstablishmentDataModelBuilder().WithEstablishmentId(701).Build(),
        ];
        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 701, 702 })),
                "section-id"
            )
            .Returns([new SubmissionEntityBuilder().WithId(100).WithEstablishmentId(702).Build()]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetGroupSubmissionInformationForSection(
            schools,
            "section-id"
        );

        // Assert
        Assert.Equal(
            [SubmissionStatus.NotStarted, SubmissionStatus.InProgress, SubmissionStatus.NotStarted],
            result.Select(information => information.Status).ToArray()
        );
    }

    [Fact]
    public async Task GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRefAsync_WhenCalled_ReturnsRepositoryResult()
    {
        // Arrange
        var expectedHistories =
            new List<(
                EstablishmentEntity Establishment,
                EstablishmentRecommendationHistoryEntity? History
            )>();
        _recommendationHistoryRepository
            .GetLatestGroupRecommendationHistoryByRecommendationRefAsync(301, "recommendation-ref")
            .Returns(expectedHistories);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result =
            await systemUnderTest.GetLatestGroupEstablishmentRecommendationHistoryByRecommendationRefAsync(
                301,
                "recommendation-ref"
            );

        // Assert
        Assert.Same(expectedHistories, result);
    }
}
