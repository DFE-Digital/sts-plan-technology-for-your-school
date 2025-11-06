using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class EstablishmentServiceTests
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow = Substitute.For<IEstablishmentWorkflow>();
    private readonly IRecommendationWorkflow _recommendationWorkflow = Substitute.For<IRecommendationWorkflow>();
    private readonly IUserWorkflow _userWorkflow = Substitute.For<IUserWorkflow>();

    private EstablishmentService CreateServiceUnderTest()
        => new EstablishmentService(
            _establishmentWorkflow,
            _recommendationWorkflow,
            _userWorkflow);

    [Fact]
    public async Task GetOrCreateEstablishmentAsync_Delegates_To_Workflow()
    {
        // Arrange
        var establishmentModel = new EstablishmentModel
        {
            Id = Guid.NewGuid(),
            DistrictAdministrativeCode = "testCode",
            LegacyId = "testLegacyId",
            Name = "Test Establishment",
            Sid = "testSid",
            Urn = "123"
        };

        var expected = new SqlEstablishmentDto
        {
            Id = 1,
            EstablishmentRef = "123",
            OrgName = "Test Establishment",
            DateCreated = DateTime.UtcNow
        };

        _establishmentWorkflow
            .GetOrCreateEstablishmentAsync(establishmentModel)
            .Returns(expected);

        var sut = CreateServiceUnderTest();

        // Act
        var actual = await sut.GetOrCreateEstablishmentAsync(establishmentModel);

        // Assert
        Assert.Same(expected, actual);
        await _establishmentWorkflow
            .Received(1)
            .GetOrCreateEstablishmentAsync(establishmentModel);
    }

    [Fact]
    public async Task GetEstablishmentLinksWithRecommendationCounts_Computes_Completed_Counts()
    {
        // Arrange
        var sections = new List<QuestionnaireSectionEntry>
        {
            new QuestionnaireSectionEntry
            {
                Sys = new SystemDetails("S1"),
                InternalName = "Test Internal Section Name 1",
                Name = "Section 1",
                CoreRecommendations = new List<RecommendationChunkEntry>
                {
                    new RecommendationChunkEntry { Sys = new SystemDetails { Id = "rec-001" } },
                    new RecommendationChunkEntry { Sys = new SystemDetails { Id = "rec-002" } },
                    new RecommendationChunkEntry { Sys = new SystemDetails { Id = "rec-003" } },
                    new RecommendationChunkEntry { Sys = new SystemDetails { Id = "rec-004" } },
                }
            },
            new QuestionnaireSectionEntry
            {
                Sys = new SystemDetails("S2"),
                InternalName = "Test Internal Section Name 2",
                Name = "Section 2",
                CoreRecommendations = new List<RecommendationChunkEntry>
                {
                    new RecommendationChunkEntry { Sys = new SystemDetails { Id = "rec-005" } },
                    new RecommendationChunkEntry { Sys = new SystemDetails { Id = "rec-006" } },
                }
            }
        };

        const int groupEstablishmentId = 101;
        var groupEstablishments = new List<SqlEstablishmentLinkDto>
        {
            new SqlEstablishmentLinkDto { Id = groupEstablishmentId, Urn = "URN-A", EstablishmentName = "A" },
            new SqlEstablishmentLinkDto { Id = 102, Urn = "URN-B", EstablishmentName = "B" }
        };
        _establishmentWorkflow.GetGroupEstablishments(groupEstablishmentId).Returns(groupEstablishments);

        var establishments = new List<SqlEstablishmentDto>
        {
            new SqlEstablishmentDto { Id = 1001, EstablishmentRef = "URN-A", OrgName = "A" },
            new SqlEstablishmentDto { Id = 1002, EstablishmentRef = "URN-B", OrgName = "B" }
        };

        _establishmentWorkflow.GetEstablishmentsByReferencesAsync(Arg.Is<IEnumerable<string>>(us => us.SequenceEqual(new[] { "URN-A", "URN-B" })))
                              .Returns(establishments);

        var recommendationsEst1 = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            ["rec-001"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = 1001,
                RecommendationId = 1,
                UserId = 1,
                NewStatus = "Complete",
                DateCreated = DateTime.UtcNow.AddDays(-1)
            },
            ["rec-002"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = 1001,
                RecommendationId = 2,
                UserId = 1,
                NewStatus = "InProgress",
                DateCreated = DateTime.UtcNow.AddDays(-2)
            },
            ["rec-003"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = 1001,
                RecommendationId = 3,
                UserId = 1,
                NewStatus = "NotStarted",
                DateCreated = DateTime.UtcNow.AddDays(-2)
            }
        };

        var recommendationsEst2 = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            ["rec-004"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = 1002,
                RecommendationId = 4,
                UserId = 1,
                NewStatus = "NotStarted",
                DateCreated = DateTime.UtcNow.AddDays(-1)
            },
            ["rec-005"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = 1002,
                RecommendationId = 5,
                UserId = 1,
                NewStatus = "InProgress",
                DateCreated = DateTime.UtcNow.AddDays(-2)
            }
        };

        _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(1001).Returns(recommendationsEst1);
        _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(1002).Returns(recommendationsEst2);

        var sut = CreateServiceUnderTest();

        // Act
        var result = await sut.GetEstablishmentLinksWithRecommendationCounts(groupEstablishmentId);

        // Assert
        var a = result.Single(x => x.Urn == "URN-A");
        var b = result.Single(x => x.Urn == "URN-B");
        Assert.Equal(2, a.InProgressOrCompletedRecommendationsCount);
        Assert.Equal(1, b.InProgressOrCompletedRecommendationsCount);
    }

    [Fact]
    public async Task RecordGroupSelection_Null_UserEstablishmentId_Creates_UserEstablishment_And_Selected_If_Missing()
    {
        // Arrange
        const string dsiRef = "testDsiReference";
        const int establishmentId = 2;
        const string orgName = "Test Establishment";
        const string urn = "123";
        const int userId = 1;

        var user = new SqlUserDto
        {
            Id = userId,
            DfeSignInRef = dsiRef
        };

        var establishmentModel = new EstablishmentModel
        {
            Id = Guid.NewGuid(),
            DistrictAdministrativeCode = "testCode",
            LegacyId = "testLegacyId",
            Name = orgName,
            Sid = "testSid",
            Urn = urn
        };

        var createdUserEstablishment = new SqlEstablishmentDto
        {
            Id = establishmentId,
            EstablishmentRef = establishmentModel.Urn,
            OrgName = establishmentModel.Name
        };

        var createdSelectedEstablishment = new SqlEstablishmentDto
        {
            Id = establishmentId,
            EstablishmentRef = urn,
            OrgName = orgName
        };

        _userWorkflow.GetUserBySignInRefAsync(dsiRef).Returns(user);
        _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentModel).Returns(createdUserEstablishment);
        _establishmentWorkflow.GetEstablishmentByReferenceAsync(urn).Returns((SqlEstablishmentDto?)null);
        _establishmentWorkflow.GetOrCreateEstablishmentAsync(urn, orgName).Returns(createdSelectedEstablishment);

        // Record and return id
        const int expectedSelectionId = 3;
        _establishmentWorkflow.RecordGroupSelection(Arg.Any<UserGroupSelectionModel>())
            .Returns(expectedSelectionId);

        var sut = CreateServiceUnderTest();

        // Act
        await sut.RecordGroupSelection(
            dsiRef,
            userEstablishmentId: null,
            userEstablishmentModel: establishmentModel,
            selectedEstablishmentUrn: urn,
            selectedEstablishmentName: orgName);

        await _userWorkflow.Received(1).GetUserBySignInRefAsync(dsiRef);

        await _establishmentWorkflow.Received(1).GetOrCreateEstablishmentAsync(establishmentModel);

        await _establishmentWorkflow.Received(1).GetEstablishmentByReferenceAsync(urn);
        await _establishmentWorkflow.Received(1).GetOrCreateEstablishmentAsync(urn, orgName);

        await _establishmentWorkflow.Received(1).RecordGroupSelection(
            Arg.Is<UserGroupSelectionModel>(m =>
                m.SelectedEstablishmentId == createdSelectedEstablishment.Id &&
                m.SelectedEstablishmentName == orgName &&
                m.UserEstablishmentId == createdUserEstablishment.Id &&
                m.UserId == user.Id));
    }

    [Fact]
    public async Task RecordGroupSelection_Throws_If_User_Not_Found()
    {
        // Arrange
        const string dsiRef = "missingUser";
        _userWorkflow.GetUserBySignInRefAsync(dsiRef)
            .Returns(Task.FromResult<SqlUserDto?>(null));

        var sut = CreateServiceUnderTest();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.RecordGroupSelection(
                dsiRef,
                userEstablishmentId: null,
                userEstablishmentModel: new EstablishmentModel { Urn = "U", Name = "N" },
                selectedEstablishmentUrn: "SURN",
                selectedEstablishmentName: "SNAME"));
    }

    [Fact]
    public async Task GetEstablishmentLinksWithRecommendationCounts_WhenSchoolNotInEstablishmentTable_IncludesSchoolWithZeroCount()
    {
        // Arrange
        const int groupEstablishmentId = 101;
        var groupEstablishments = new List<SqlEstablishmentLinkDto>
        {
            new SqlEstablishmentLinkDto { Id = groupEstablishmentId, Urn = "URN-A", EstablishmentName = "School A" },
            new SqlEstablishmentLinkDto { Id = 102, Urn = "URN-B", EstablishmentName = "School B" },
            new SqlEstablishmentLinkDto { Id = 103, Urn = "URN-C", EstablishmentName = "School C (Never Logged In)" }
        };
        _establishmentWorkflow.GetGroupEstablishments(groupEstablishmentId).Returns(groupEstablishments);

        // Only URN-A and URN-B exist in establishment table (URN-C has never logged in)
        var establishments = new List<SqlEstablishmentDto>
        {
            new SqlEstablishmentDto { Id = 1001, EstablishmentRef = "URN-A", OrgName = "School A" },
            new SqlEstablishmentDto { Id = 1002, EstablishmentRef = "URN-B", OrgName = "School B" }
        };

        _establishmentWorkflow.GetEstablishmentsByReferencesAsync(Arg.Is<IEnumerable<string>>(us => us.SequenceEqual(new[] { "URN-A", "URN-B", "URN-C" })))
                              .Returns(establishments);

        var recommendationsEst1 = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            ["rec-001"] = new SqlEstablishmentRecommendationHistoryDto
            {
                EstablishmentId = 1001,
                RecommendationId = 1,
                UserId = 1,
                NewStatus = "Complete",
                DateCreated = DateTime.UtcNow
            }
        };

        _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(1001).Returns(recommendationsEst1);
        _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(1002).Returns(new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>());

        var sut = CreateServiceUnderTest();

        // Act
        var result = await sut.GetEstablishmentLinksWithRecommendationCounts(groupEstablishmentId);

        // Assert
        Assert.Equal(3, result.Count); // All 3 schools should be returned

        var schoolA = result.Single(x => x.Urn == "URN-A");
        var schoolB = result.Single(x => x.Urn == "URN-B");
        var schoolC = result.Single(x => x.Urn == "URN-C");

        Assert.Equal(1, schoolA.InProgressOrCompletedRecommendationsCount);
        Assert.Equal(0, schoolB.InProgressOrCompletedRecommendationsCount);
        Assert.Equal(0, schoolC.InProgressOrCompletedRecommendationsCount); // School C should have 0 count, not be excluded
    }
}
