using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class EstablishmentServiceTests
{
    private readonly ILogger<EstablishmentService> _logger = Substitute.For<ILogger<EstablishmentService>>();
    private readonly IEstablishmentWorkflow _establishmentWorkflow = Substitute.For<IEstablishmentWorkflow>();
    private readonly ISubmissionWorkflow _submissionWorkflow = Substitute.For<ISubmissionWorkflow>();
    private readonly IUserWorkflow _userWorkflow = Substitute.For<IUserWorkflow>();

    private EstablishmentService CreateServiceUnderTest()
        => new EstablishmentService(
            _logger,
            _establishmentWorkflow,
            _submissionWorkflow,
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
    public async Task GetEstablishmentLinksWithSubmissionStatusesAndCounts_Computes_Completed_Counts()
    {
        // Arrange
        var categories = new List<QuestionnaireCategoryEntry>
        {
            new QuestionnaireCategoryEntry
            {
                InternalName = "Test Internal Category Name",
                Sections = new List<QuestionnaireSectionEntry>
                {
                    new QuestionnaireSectionEntry
                    {
                        Sys = new SystemDetails("S1"),
                        InternalName = "Test Internal Section Name 1",
                        Name = "Section 1"
                    },
                    new QuestionnaireSectionEntry {
                        Sys = new SystemDetails("S2"),
                        InternalName = "Test Internal Section Name 2",
                        Name = "Section 2"
                    }
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

        _submissionWorkflow.GetSectionStatusesAsync(1001, Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "S1", "S2" })))
                           .Returns([
                               new SqlSectionStatusDto { Completed = true },
                               new SqlSectionStatusDto { Completed = false, LastCompletionDate = DateTime.UtcNow }
                           ]);

        _submissionWorkflow.GetSectionStatusesAsync(1002, Arg.Any<IEnumerable<string>>())
                           .Returns([
                               new SqlSectionStatusDto { Completed = false },
                               new SqlSectionStatusDto { Completed = false }
                           ]);

        var sut = CreateServiceUnderTest();

        // Act
        var result = await sut.GetEstablishmentLinksWithSubmissionStatusesAndCounts(categories, groupEstablishmentId);

        // Assert
        var a = result.Single(x => x.Urn == "URN-A");
        var b = result.Single(x => x.Urn == "URN-B");
        Assert.Equal(2, a.CompletedSectionsCount);
        Assert.Equal(0, b.CompletedSectionsCount);
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
}
