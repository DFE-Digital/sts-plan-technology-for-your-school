using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class EstablishmentServiceTests
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        Substitute.For<IEstablishmentWorkflow>();
    private readonly IEstablishmentRepository _establishmentRepository =
        Substitute.For<IEstablishmentRepository>();
    private readonly IRecommendationWorkflow _recommendationWorkflow =
        Substitute.For<IRecommendationWorkflow>();
    private readonly IUserWorkflow _userWorkflow = Substitute.For<IUserWorkflow>();
    private static readonly string[] urnAandB = ["URN-A", "URN-B"];
    private static readonly string[] urnAandBandC = ["URN-A", "URN-B", "URN-C"];

    private EstablishmentService CreateServiceUnderTest() =>
        new EstablishmentService(_establishmentWorkflow, _establishmentRepository, _recommendationWorkflow, _userWorkflow);

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
            Urn = "123",
        };

        var expected = new SqlEstablishmentDto
        {
            Id = 1,
            EstablishmentRef = "123",
            OrgName = "Test Establishment",
            DateCreated = DateTime.UtcNow,
        };

        _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentModel).Returns(expected);

        var sut = CreateServiceUnderTest();

        // Act
        var actual = await sut.GetOrCreateEstablishmentAsync(establishmentModel);

        // Assert
        Assert.Same(expected, actual);
        await _establishmentWorkflow.Received(1).GetOrCreateEstablishmentAsync(establishmentModel);
    }

    [Fact]
    public async Task GetEstablishmentByIdAsync_Returns_Dto_When_Establishment_Exists()
    {
        var sut = CreateServiceUnderTest();

        var entity = new EstablishmentEntity
        {
            Id = 123,
            EstablishmentRef = "URN-123",
            OrgName = "School 123",
            GroupUid = "500"
        };

        _establishmentRepository.GetEstablishmentByIdAsync(123).Returns(entity);

        var result = await sut.GetEstablishmentByIdAsync(123);

        Assert.Equal(123, result.Id);
        Assert.Equal("URN-123", result.EstablishmentRef);
        Assert.Equal("School 123", result.OrgName);
        Assert.Equal("500", result.GroupUid);
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

        var user = new SqlUserDto { Id = userId, DfeSignInRef = dsiRef };

        var establishmentModel = new EstablishmentModel
        {
            Id = Guid.NewGuid(),
            DistrictAdministrativeCode = "testCode",
            LegacyId = "testLegacyId",
            Name = orgName,
            Sid = "testSid",
            Urn = urn,
        };

        var createdUserEstablishment = new SqlEstablishmentDto
        {
            Id = establishmentId,
            EstablishmentRef = establishmentModel.Urn,
            OrgName = establishmentModel.Name,
        };

        var createdSelectedEstablishment = new SqlEstablishmentDto
        {
            Id = establishmentId,
            EstablishmentRef = urn,
            OrgName = orgName,
        };

        _userWorkflow.GetUserBySignInRefAsync(dsiRef).Returns(user);
        _establishmentWorkflow
            .GetOrCreateEstablishmentAsync(establishmentModel)
            .Returns(createdUserEstablishment);
        _establishmentWorkflow
            .GetEstablishmentByReferenceAsync(urn)
            .Returns((SqlEstablishmentDto?)null);
        _establishmentWorkflow
            .GetOrCreateEstablishmentAsync(urn, orgName)
            .Returns(createdSelectedEstablishment);

        // Record and return id
        const int expectedSelectionId = 3;
        _establishmentWorkflow
            .RecordGroupSelection(Arg.Any<UserGroupSelectionModel>())
            .Returns(expectedSelectionId);

        var sut = CreateServiceUnderTest();

        // Act
        await sut.RecordGroupSelection(
            dsiRef,
            userEstablishmentId: null,
            userEstablishmentModel: establishmentModel,
            selectedEstablishmentUrn: urn,
            selectedEstablishmentName: orgName
        );

        await _userWorkflow.Received(1).GetUserBySignInRefAsync(dsiRef);

        await _establishmentWorkflow.Received(1).GetOrCreateEstablishmentAsync(establishmentModel);

        await _establishmentWorkflow.Received(1).GetEstablishmentByReferenceAsync(urn);
        await _establishmentWorkflow.Received(1).GetOrCreateEstablishmentAsync(urn, orgName);

        await _establishmentWorkflow
            .Received(1)
            .RecordGroupSelection(
                Arg.Is<UserGroupSelectionModel>(m =>
                    m.SelectedEstablishmentId == createdSelectedEstablishment.Id
                    && m.SelectedEstablishmentName == orgName
                    && m.UserEstablishmentId == createdUserEstablishment.Id
                    && m.UserId == user.Id
                )
            );
    }

    [Fact]
    public async Task RecordGroupSelection_Throws_If_User_Not_Found()
    {
        // Arrange
        const string dsiRef = "missingUser";
        _userWorkflow.GetUserBySignInRefAsync(dsiRef).Returns(Task.FromResult<SqlUserDto?>(null));

        var sut = CreateServiceUnderTest();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.RecordGroupSelection(
                dsiRef,
                userEstablishmentId: null,
                userEstablishmentModel: new EstablishmentModel { Urn = "U", Name = "N" },
                selectedEstablishmentUrn: "SURN",
                selectedEstablishmentName: "SNAME"
            )
        );
    }

    [Fact]
    public async Task GetEstablishmentByIdAsync_Throws_When_Establishment_Does_Not_Exist()
    {
        var sut = CreateServiceUnderTest();

        _establishmentRepository.GetEstablishmentByIdAsync(999).Returns((EstablishmentEntity?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetEstablishmentByIdAsync(999));

        Assert.Equal("Establishment with id 999 not found", ex.Message);
    }
}
