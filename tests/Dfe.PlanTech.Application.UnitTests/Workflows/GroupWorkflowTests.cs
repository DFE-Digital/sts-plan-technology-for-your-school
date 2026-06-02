using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class GroupWorkflowTests
{
    private readonly ISubmissionRepository _submissionRepository =
        Substitute.For<ISubmissionRepository>();

    private GroupWorkflow CreateServiceUnderTest() => new(_submissionRepository);

    private static EstablishmentEntity BuildEstablishment(int id = 1)
    {
        return new EstablishmentEntity
        {
            Id = id,
            EstablishmentRef = $"testRef{id}",
            OrgName = $"testName{id}",
            DateCreated = DateTime.UtcNow,
        };
    }

    private static SubmissionEntity BuildSubmission(
        int id,
        int establishmentId,
        string sectionId,
        SubmissionStatus status = SubmissionStatus.CompleteReviewed
    )
    {
        return new SubmissionEntity
        {
            Id = id,
            EstablishmentId = establishmentId,
            Establishment = BuildEstablishment(establishmentId),
            SectionId = sectionId,
            SectionName = $"Section {sectionId}",
            Status = status,
            Responses = new List<ResponseEntity>(),
        };
    }

    [Fact]
    public async Task GetGroupCompletedSubmissions_CallsRepository()
    {
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2, 3 };

        _submissionRepository
            .GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(establishmentIds)
            .Returns(new List<SubmissionEntity>());

        await sut.GetGroupCompletedSubmissions(establishmentIds);

        await _submissionRepository
            .Received(1)
            .GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(
                Arg.Is<int[]>(ids => ids.SequenceEqual(establishmentIds))
            );
    }

    [Fact]
    public async Task GetGroupCompletedSubmissions_ReturnsSubmissionDtos()
    {
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2 };

        var submissions = new List<SubmissionEntity>
        {
            BuildSubmission(
                id: 100,
                establishmentId: 1,
                sectionId: "SEC-1"
            ),
            BuildSubmission(
                id: 200,
                establishmentId: 2,
                sectionId: "SEC-2"
            ),
        };

        _submissionRepository
            .GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(establishmentIds)
            .Returns(submissions);

        var result = await sut.GetGroupCompletedSubmissions(establishmentIds);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(100, submission.Id);
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal("SEC-1", submission.SectionId);
                Assert.Equal(SubmissionStatus.CompleteReviewed, submission.Status);
            },
            submission =>
            {
                Assert.Equal(200, submission.Id);
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal("SEC-2", submission.SectionId);
                Assert.Equal(SubmissionStatus.CompleteReviewed, submission.Status);
            }
        );
    }

    [Fact]
    public async Task GetGroupCompletedSubmissions_WhenNoSubmissions_ReturnsEmptyList()
    {
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2, 3 };

        _submissionRepository
            .GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(establishmentIds)
            .Returns(new List<SubmissionEntity>());

        var result = await sut.GetGroupCompletedSubmissions(establishmentIds);

        Assert.NotNull(result);
        Assert.Empty(result);

        await _submissionRepository
            .Received(1)
            .GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(establishmentIds);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_CallsSubmissionRepository()
    {
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2, 3 };
        var sectionId = "sec1";

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            .Returns(new List<SubmissionEntity>());

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentIds, sectionId);

        await _submissionRepository
            .Received(1)
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_ReturnsSubmissionInformationModelsForAllSchools()
    {
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2, 3, 4 };
        var sectionId = "sec1";

        var submissions = new List<SubmissionEntity>
        {
            BuildSubmission(
                id: 100,
                establishmentId: 1,
                sectionId
            ),
            BuildSubmission(
                id: 200,
                establishmentId: 2,
                sectionId
            ),
        };

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            .Returns(submissions);

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentIds, sectionId);

        Assert.NotNull(result);
        Assert.Equal(4, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(1, submission.EstablishmentId);
            },
            submission =>
            {
                Assert.Equal(2, submission.EstablishmentId);
            }, submission =>
            {
                Assert.Equal(3, submission.EstablishmentId);
            }, submission =>
            {
                Assert.Equal(4, submission.EstablishmentId);
            }
        );
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_ReturnsNotStartedWhereNoSubmissionExistsForSchool()
    {
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2 };
        var sectionId = "sec1";

        var submissions = new List<SubmissionEntity>
        {
            BuildSubmission(
                id: 200,
                establishmentId: 2,
                sectionId,
                status: SubmissionStatus.InProgress
            ),
        };

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            .Returns(submissions);

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentIds, sectionId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.NotStarted, submission.Status);
            },
            submission =>
            {
                Assert.Equal(200, submission.SubmissionId);
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.InProgress, submission.Status);
            }
        );
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_ReturnsCorrectSubmissionInfo()
    { 
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2 };
        var sectionId = "sec1";
        var sectionName = "Section 1";

        var submission1 = new SubmissionEntity()
        {
            Id = 100,
            EstablishmentId = 1,
            SectionId = sectionId,
            SectionName = sectionName,
            Status = SubmissionStatus.InProgress,
            DateCreated = new DateTime(2025, 1, 1),
            DateLastUpdated = new DateTime(2025, 2, 1)
        };

        var submission2 = new SubmissionEntity()
        {
            Id = 200,
            EstablishmentId = 2,
            SectionId = sectionId,
            SectionName = sectionName,
            Status = SubmissionStatus.CompleteReviewed,
            DateCreated = new DateTime(2025, 3, 1),
            DateLastUpdated = new DateTime(2025, 4, 1),
            DateCompleted = new DateTime(2025, 5, 1)
        };


        var submissions = new List<SubmissionEntity>
        {
            submission1,
            submission2
        };

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            .Returns(submissions);

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentIds, sectionId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(100, submission.SubmissionId);
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.InProgress, submission.Status);
                Assert.Equal(new DateTime(2025, 1, 1), submission.DateCreated);
                Assert.Equal(new DateTime(2025, 2, 1), submission.DateLastUpdated);
            },
            submission =>
            {
                Assert.Equal(200, submission.SubmissionId);
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.CompleteReviewed, submission.Status);
                Assert.Equal(new DateTime(2025, 3, 1), submission.DateCreated);
                Assert.Equal(new DateTime(2025, 4, 1), submission.DateLastUpdated);
                Assert.Equal(new DateTime(2025, 5, 1), submission.DateCompleted);
            }
        );
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_SubmissionRepository_Is_Null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new GroupWorkflow(null!));

        Assert.Equal("submissionRepository", exception.ParamName);
    }
}
