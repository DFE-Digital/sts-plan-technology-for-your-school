using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class EstablishmentWorkflowTests
{
    private readonly FakeLogger<EstablishmentWorkflow> _logger = new();
    private readonly IEstablishmentRepository _establishmentRepository =
        Substitute.For<IEstablishmentRepository>();
    private readonly IGroupReadActivityRepository _groupReadActivityRepository =
        Substitute.For<IGroupReadActivityRepository>();
    private readonly IGiasRepository _giasRepository = Substitute.For<IGiasRepository>();

    private EstablishmentWorkflow CreateSystemUnderTest() =>
        new(_logger, _establishmentRepository, _groupReadActivityRepository, _giasRepository);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<EstablishmentWorkflow>(
            _logger,
            _establishmentRepository,
            _groupReadActivityRepository,
            _giasRepository
        );
    }

    [Fact]
    public async Task GetOrCreateEstablishmentAsync_WhenEstablishmentExists_ReturnsStoredRecord()
    {
        // Arrange
        var establishmentModel = new EstablishmentModelBuilder().WithUid("100001").Build();
        var expectedEstablishment = new EstablishmentEntityBuilder()
            .WithId(701)
            .WithEstablishmentRef("100001")
            .WithOrgName("Northfield Primary")
            .Build();
        _establishmentRepository
            .GetEstablishmentByReferenceAsync("100001")
            .Returns(expectedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetOrCreateEstablishmentAsync(establishmentModel);

        // Assert
        Assert.Equal(expectedEstablishment.Id, result.Id);
        Assert.Equal(expectedEstablishment.EstablishmentRef, result.EstablishmentRef);
        Assert.Equal(expectedEstablishment.OrgName, result.OrgName);
        await _establishmentRepository
            .DidNotReceive()
            .CreateEstablishmentFromModelAsync(Arg.Any<EstablishmentModel>());
    }

    [Fact]
    public async Task GetOrCreateEstablishmentAsync_WhenEstablishmentIsMissing_CreatesRecord()
    {
        // Arrange
        var establishmentModel = new EstablishmentModelBuilder().WithUid("100002").Build();
        var createdEstablishment = new EstablishmentEntityBuilder()
            .WithId(702)
            .WithEstablishmentRef("100002")
            .Build();
        _establishmentRepository
            .GetEstablishmentByReferenceAsync("100002")
            .Returns((EstablishmentEntity?)null);
        _establishmentRepository
            .CreateEstablishmentFromModelAsync(establishmentModel)
            .Returns(createdEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetOrCreateEstablishmentAsync(establishmentModel);

        // Assert
        Assert.Equal(createdEstablishment.Id, result.Id);
        Assert.Equal(createdEstablishment.EstablishmentRef, result.EstablishmentRef);
        await _establishmentRepository
            .Received(1)
            .CreateEstablishmentFromModelAsync(establishmentModel);
    }

    [Fact]
    public async Task GetOrCreateSchoolAsync_WhenUrnIsNotNumeric_ThrowsInvalidDataException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.GetOrCreateSchoolAsync("school-urn", "Northfield Primary")
        );

        // Assert
        Assert.Equal("Cannot parse an integer URN from the provided string", exception.Message);
        await _giasRepository.DidNotReceive().GetSchoolByUrn(Arg.Any<int>());
    }

    [Fact]
    public async Task GetOrCreateSchoolAsync_WhenSchoolIsMissingFromGias_CreatesRecord()
    {
        // Arrange
        _giasRepository.GetSchoolByUrn(100003).Returns((GiasEstablishmentEntity?)null);
        _establishmentRepository
            .GetEstablishmentByReferenceAsync("100003")
            .Returns((EstablishmentEntity?)null);
        _establishmentRepository
            .CreateEstablishmentFromModelAsync(
                Arg.Is<EstablishmentModel>(model =>
                    model.Urn == "100003" && model.Name == "Northfield Primary"
                )
            )
            .Returns(
                new EstablishmentEntityBuilder()
                    .WithId(703)
                    .WithEstablishmentRef("100003")
                    .WithOrgName("Northfield Primary")
                    .Build()
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetOrCreateSchoolAsync("100003", "Northfield Primary");

        // Assert
        Assert.Equal(703, result.Id);
        Assert.Equal("Northfield Primary", result.OrgName);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Warning, logRecord.Level);
        Assert.Contains("Could not find establishment", logRecord.Message);
    }

    [Fact]
    public async Task RecordGroupSelectionAsync_WhenCalled_RecordsSelection()
    {
        // Arrange
        var selection = new UserGroupSelectionModel
        {
            UserId = 42,
            UserEstablishmentId = 301,
            SelectedEstablishmentId = 701,
            SelectedEstablishmentName = "Northfield Primary",
        };
        _groupReadActivityRepository.RecordGroupSelectionAsync(selection).Returns(9001);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RecordGroupSelectionAsync(selection);

        // Assert
        Assert.Equal(9001, result);
        await _groupReadActivityRepository.Received(1).RecordGroupSelectionAsync(selection);
    }
}
