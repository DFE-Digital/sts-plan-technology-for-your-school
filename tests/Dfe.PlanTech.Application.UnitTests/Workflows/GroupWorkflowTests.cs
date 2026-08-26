using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class GroupWorkflowTests
{
    private readonly ISubmissionRepository _submissionRepository =
        Substitute.For<ISubmissionRepository>();
    private readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();
    
    private GroupWorkflow CreateServiceUnderTest() => new(_submissionRepository, _establishmentService);

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
        // Arrange
        var sut = CreateServiceUnderTest();

        var sectionId = "sec1";

        var establishmentLink1 = new SqlEstablishmentLinkDto()
        {
            Urn = "testRef1",
            EstablishmentName = "testName1"
        };

        var establishmentLink2 = new SqlEstablishmentLinkDto()
        {
            Urn = "testRef2",
            EstablishmentName = "testName2"
        };

        var establishmentLink3 = new SqlEstablishmentLinkDto()
        {
            Urn = "testRef3",
            EstablishmentName = "testName3"
        };

        var establishmentLinks = new List<SqlEstablishmentLinkDto>()
        {
            establishmentLink1,
            establishmentLink2,
            establishmentLink3
        };

        var establishment1 = new SqlEstablishmentDto()
        {
            Id = 1,
            EstablishmentRef = "testRef1",
            OrgName = "testName1"
        };
        var establishment2 = new SqlEstablishmentDto()
        {
            Id = 2,
            EstablishmentRef = "testRef2",
            OrgName = "testName2"
        };
        var establishment3 = new SqlEstablishmentDto()
        {
            Id = 3,
            EstablishmentRef = "testRef3",
            OrgName = "testName3"
        };

        var establishmentIds = new int[] { 1, 2, 3 }; 

        _establishmentService.GetOrCreateEstablishmentAsync("testRef1", "testName1").Returns(establishment1);
        _establishmentService.GetOrCreateEstablishmentAsync("testRef2", "testName2").Returns(establishment2);
        _establishmentService.GetOrCreateEstablishmentAsync("testRef3", "testName3").Returns(establishment3);

        // Act
        var result = await sut.GetGroupSubmissionInformationForSection(establishmentLinks, sectionId);

        // Assert
        await _submissionRepository
            .Received(1)
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Is<int[]>(ids => ids.SequenceEqual(establishmentIds)),
                sectionId);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_ReturnsSubmissionInformationModelsForAllSchools()
    {
        var sut = CreateServiceUnderTest();

        var sectionId = "sec1";

        var establishmentLinks = new List<SqlEstablishmentLinkDto>()
        {
            new()
            {
                Urn = "testRef1",
                EstablishmentName = "testName1"
            },
            new()
            {
                Urn = "testRef2",
                EstablishmentName = "testName2"
            },
            new()
            {
                Urn = "testRef3",
                EstablishmentName = "testName3"
            },
            new()
            {
                Urn = "testRef4",
                EstablishmentName = "testName4"
            }
        };

        var establishment1 = new SqlEstablishmentDto()
        {
            Id = 1,
            EstablishmentRef = "testRef1",
            OrgName = "testName1"
        };
        var establishment2 = new SqlEstablishmentDto()
        {
            Id = 2,
            EstablishmentRef = "testRef2",
            OrgName = "testName2"
        };
        var establishment3 = new SqlEstablishmentDto()
        {
            Id = 3,
            EstablishmentRef = "testRef3",
            OrgName = "testName3"
        };
        var establishment4 = new SqlEstablishmentDto()
        {
            Id = 4,
            EstablishmentRef = "testRef4",
            OrgName = "testName4"
        };
        var establishmentIds = new int[] { 1, 2, 3, 4 };

        _establishmentService.GetOrCreateEstablishmentAsync("testRef1", "testName1").Returns(establishment1);
        _establishmentService.GetOrCreateEstablishmentAsync("testRef2", "testName2").Returns(establishment2);
        _establishmentService.GetOrCreateEstablishmentAsync("testRef3", "testName3").Returns(establishment3);
        _establishmentService.GetOrCreateEstablishmentAsync("testRef4", "testName4").Returns(establishment4);

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

        submissions[0].Establishment = new EstablishmentEntity { Id = 1, EstablishmentRef = "testRef1", OrgName = "Test 1" };
        submissions[1].Establishment = new EstablishmentEntity { Id = 2, EstablishmentRef = "testRef2", OrgName = "Test 2" };

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Is<int[]>(x => x.SequenceEqual(establishmentIds)),
                sectionId)
            .Returns(submissions);

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentLinks, sectionId);

        Assert.NotNull(result);
        Assert.Equal(4, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal("testName1", submission.EstablishmentName);
                Assert.Equal(SubmissionStatus.CompleteReviewed, submission.Status);
            },
            submission =>
            {
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal("testName2", submission.EstablishmentName);
                Assert.Equal(SubmissionStatus.CompleteReviewed, submission.Status);
            },
            submission =>
            {
                Assert.Equal(3, submission.EstablishmentId);
                Assert.Equal("testName3", submission.EstablishmentName);
                Assert.Equal(SubmissionStatus.NotStarted, submission.Status);
            },
            submission =>
            {
                Assert.Equal(4, submission.EstablishmentId);
                Assert.Equal("testName4", submission.EstablishmentName);
                Assert.Equal(SubmissionStatus.NotStarted, submission.Status);
            }
        );
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_ReturnsNotStartedWhereNoSubmissionExistsForSchool()
    {
        var sut = CreateServiceUnderTest();
        var sectionId = "sec2";

        var establishmentLink1 = new SqlEstablishmentLinkDto()
        {
            Urn = "testRef1",
            EstablishmentName = "testName1"
        };

        var establishmentLink2 = new SqlEstablishmentLinkDto()
        {
            Urn = "testRef2",
            EstablishmentName = "testName2"
        };

        var establishmentLinks = new List<SqlEstablishmentLinkDto>()
        {
            establishmentLink1,
            establishmentLink2,
        };

        var establishment1 = new SqlEstablishmentDto()
        {
            Id = 1,
            EstablishmentRef = "testRef1",
            OrgName = "testName1"
        };
        var establishment2 = new SqlEstablishmentDto()
        {
            Id = 2,
            EstablishmentRef = "testRef2",
            OrgName = "testName2"
        };

        var establishmentIds = new int[] { 1, 2 };

        _establishmentService.GetOrCreateEstablishmentAsync("testRef1", "testName1").Returns(establishment1);
        _establishmentService.GetOrCreateEstablishmentAsync("testRef2", "testName2").Returns(establishment2);

        var submissions = new List<SubmissionEntity>
        {
            BuildSubmission(
                id: 200,
                establishmentId: 2,
                sectionId,
                status: SubmissionStatus.InProgress
            ),
        };

        submissions[0].Establishment = new EstablishmentEntity { Id = 2, EstablishmentRef = "testRef2", OrgName = "testName2" };

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Is<int[]>(x => x.SequenceEqual(establishmentIds)),
                sectionId)
            .Returns(submissions);

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            .Returns(submissions);

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentLinks, sectionId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal("testName1", submission.EstablishmentName);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.NotStarted, submission.Status);
            },
            submission =>
            {
                Assert.Equal(200, submission.SubmissionId);
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal("testName2", submission.EstablishmentName);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.InProgress, submission.Status);
            }
        );
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_ReturnsCorrectSubmissionInfo()
    { 
        var sut = CreateServiceUnderTest();
        var sectionId = "sec3";

        var establishmentLink1 = new SqlEstablishmentLinkDto()
        {
            Urn = "testRef1",
            EstablishmentName = "Test 1"
        };

        var establishmentLink2 = new SqlEstablishmentLinkDto()
        {
            Urn = "testRef2",
            EstablishmentName = "Test 2"
        };

        var establishmentLinks = new List<SqlEstablishmentLinkDto>()
        {
            establishmentLink1,
            establishmentLink2,
        };

        var establishment1 = new SqlEstablishmentDto()
        {
            Id = 1,
            EstablishmentRef = "testRef1",
            OrgName = "Test 1"
        };
        var establishment2 = new SqlEstablishmentDto()
        {
            Id = 2,
            EstablishmentRef = "testRef2",
            OrgName = "Test 2"
        };

        var establishmentIds = new int[] { 1, 2 };
        var establishmentRefs = new[] { "testRef1", "testRef2" };

        _establishmentService.GetOrCreateEstablishmentAsync("testRef1", "Test 1").Returns(establishment1);
        _establishmentService.GetOrCreateEstablishmentAsync("testRef2", "Test 2").Returns(establishment2);

        var submissions = new List<SubmissionEntity>
        {
            BuildSubmission(
                id: 101,
                establishmentId: 1,
                sectionId,
                status: SubmissionStatus.InProgress),
            BuildSubmission(
                id: 201,
                establishmentId: 2,
                sectionId,
                status: SubmissionStatus.CompleteReviewed)
        };

        submissions[0].Establishment = new EstablishmentEntity { Id = 1, EstablishmentRef = "testRef1", OrgName = "Test 1" }; ;
        submissions[0].DateCreated = new DateTime(2025, 1, 1);
        submissions[0].DateLastUpdated = new DateTime(2025, 2, 1);
        submissions[1].Establishment = new EstablishmentEntity { Id = 2, EstablishmentRef = "testRef2", OrgName = "Test 2" }; ;
        submissions[1].DateCreated = new DateTime(2025, 3, 1);
        submissions[1].DateLastUpdated = new DateTime(2025, 4, 1);
        submissions[1].DateCompleted = new DateTime(2025, 5, 1);

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Is<int[]>(x => x.SequenceEqual(establishmentIds)),
                sectionId)
            .Returns(submissions);

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentLinks, sectionId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(101, submission.SubmissionId);
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal("Test 1", submission.EstablishmentName);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.InProgress, submission.Status);
                Assert.Equal(DateTimeHelper.FormattedDateShort(new DateTime(2025, 1, 1)), submission.DateCreated);
                Assert.Equal(DateTimeHelper.FormattedDateShort(new DateTime(2025, 2, 1)), submission.DateLastUpdated);
            },
            submission =>
            {
                Assert.Equal(201, submission.SubmissionId);
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal("Test 2", submission.EstablishmentName);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.CompleteReviewed, submission.Status);
                Assert.Equal(DateTimeHelper.FormattedDateShort(new DateTime(2025, 3, 1)), submission.DateCreated);
                Assert.Equal(DateTimeHelper.FormattedDateShort(new DateTime(2025, 4, 1)), submission.DateLastUpdated);
                Assert.Equal(DateTimeHelper.FormattedDateShort(new DateTime(2025, 5, 1)), submission.DateCompleted);
            }
        );
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_SubmissionRepository_Is_Null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new GroupWorkflow(null!, _establishmentService));

        Assert.Equal("submissionRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_EstablishmentService_Is_Null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new GroupWorkflow(_submissionRepository, null!));

        Assert.Equal("establishmentService", exception.ParamName);
    }
}
