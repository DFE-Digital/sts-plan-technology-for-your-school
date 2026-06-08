using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class GroupWorkflowTests
{
    private readonly ISubmissionRepository _submissionRepository =
        Substitute.For<ISubmissionRepository>();
    private readonly IEstablishmentRepository _establishmentRepository =
        Substitute.For<IEstablishmentRepository>();
    
    private GroupWorkflow CreateServiceUnderTest() => new(_submissionRepository, _establishmentRepository);

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

        var sectionId = "sec1";
        var establishmentRefs = new[] { "testRef10", "testRef20", "testRef30" };
        var establishments = new List<EstablishmentEntity>()
        {
            BuildEstablishment(10),
            BuildEstablishment(20),
            BuildEstablishment(30)
        };

        var establishmentIds = new int[] { 10, 20, 30 };

        _establishmentRepository.GetEstablishmentsByReferencesAsync(establishmentRefs)
            .Returns(establishments);

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            .Returns(new List<SubmissionEntity>());

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentRefs, sectionId);

        await _submissionRepository
            .Received(1)
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId);
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_ReturnsSubmissionInformationModelsForAllSchools()
    {
        var sut = CreateServiceUnderTest();

        var establishmentRefs = new[] { "testRef1", "testRef2", "testRef3", "testRef4" };
        var establishmentIds = new[] { 1, 2, 3, 4 };
        var sectionId = "sec1";
        var establishment1 = BuildEstablishment(1);
        var establishment2 = BuildEstablishment(2);
        var establishment3 = BuildEstablishment(3);
        var establishment4 = BuildEstablishment(4);

        var establishments = new List<EstablishmentEntity>()
        {
            establishment1,
            establishment2,
            establishment3,
            establishment4
        };

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

        submissions[0].Establishment = establishment1;
        submissions[1].Establishment = establishment2;

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Is<int[]>(x => x.SequenceEqual(new[] { 1, 2, 3, 4 })),
                sectionId)
            .Returns(submissions);

        _establishmentRepository.GetEstablishmentsByReferencesAsync(establishmentRefs)
            .Returns(establishments);
        _establishmentRepository.GetEstablishmentByReferenceAsync("testRef3")
            .Returns(establishment3);
        _establishmentRepository.GetEstablishmentByReferenceAsync("testRef4")
            .Returns(establishment4);


        var result = await sut.GetGroupSubmissionInformationForSection(establishmentRefs, sectionId);

        Assert.NotNull(result);
        Assert.Equal(4, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal("testName1", submission.EstablishmentName);
            },
            submission =>
            {
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal("testName2", submission.EstablishmentName);
            },
            submission =>
            {
                Assert.Equal(3, submission.EstablishmentId);
                Assert.Equal("testName3", submission.EstablishmentName);
            },
            submission =>
            {
                Assert.Equal(4, submission.EstablishmentId);
                Assert.Equal("testName4", submission.EstablishmentName);
            }
        );
    }

    [Fact]
    public async Task GetGroupSubmissionInformationForSection_ReturnsNotStartedWhereNoSubmissionExistsForSchool()
    {
        var sut = CreateServiceUnderTest();

        var establishmentRefs = new string[] { "testRef1", "testRef2" };
        var establishment1 = BuildEstablishment(1);
        var establishment2 = BuildEstablishment(2);
        var establishments = new List<EstablishmentEntity>()
        {
            establishment1,
            establishment2
        };

        var establishmentIds = establishments.Select(est => est.Id).ToList();

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

        submissions[0].Establishment = establishment2;
        
        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(
                Arg.Is<int[]>(x => x.SequenceEqual(new[] { 1, 2 })),
                sectionId)
            .Returns(submissions);

        _establishmentRepository.GetEstablishmentsByReferencesAsync(establishmentRefs)
            .Returns(establishments);
        _establishmentRepository.GetEstablishmentByReferenceAsync("testRef1")
            .Returns(establishment1);

        _submissionRepository
            .GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            .Returns(submissions);

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentRefs, sectionId);

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

        var establishmentIds = new[] { 1, 2 };
        var establishmentRefs = new[] { "testRef1", "testRef2" };
        var sectionId = "sec1";
        var sectionName = "Section 1";

        var submission1 = new SubmissionEntity()
        {
            Id = 100,
            EstablishmentId = 1,
            Establishment = BuildEstablishment(1),
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
            Establishment = BuildEstablishment(2),
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

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentRefs, sectionId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            submission =>
            {
                Assert.Equal(100, submission.SubmissionId);
                Assert.Equal(1, submission.EstablishmentId);
                Assert.Equal("testName1", submission.EstablishmentName);
                Assert.Equal(sectionId, submission.SectionId);
                Assert.Equal(SubmissionStatus.InProgress, submission.Status);
                Assert.Equal(new DateTime(2025, 1, 1), submission.DateCreated);
                Assert.Equal(new DateTime(2025, 2, 1), submission.DateLastUpdated);
            },
            submission =>
            {
                Assert.Equal(200, submission.SubmissionId);
                Assert.Equal(2, submission.EstablishmentId);
                Assert.Equal("testName2", submission.EstablishmentName);
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
        var exception = Assert.Throws<ArgumentNullException>(() => new GroupWorkflow(null!, _establishmentRepository));

        Assert.Equal("submissionRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_EstablishmentRepository_Is_Null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new GroupWorkflow(_submissionRepository, null!));

        Assert.Equal("submissionRepository", exception.ParamName);
    }
}
