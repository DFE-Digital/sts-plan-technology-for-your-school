using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class EstablishmentServiceTests
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        Substitute.For<IEstablishmentWorkflow>();
    private readonly IRecommendationWorkflow _recommendationWorkflow =
        Substitute.For<IRecommendationWorkflow>();
    private readonly IUserWorkflow _userWorkflow = Substitute.For<IUserWorkflow>();

    private EstablishmentService CreateSystemUnderTest() =>
        new(_establishmentWorkflow, _recommendationWorkflow, _userWorkflow);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<EstablishmentService>(
            _establishmentWorkflow,
            _recommendationWorkflow,
            _userWorkflow
        );
    }

    [Fact]
    public async Task GetOrCreateEstablishmentAsync_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        var establishmentModel = new EstablishmentModelBuilder().WithUrn("100301").Build();
        var expectedEstablishment = new SqlEstablishmentDtoBuilder().WithId(301).Build();
        _establishmentWorkflow
            .GetOrCreateEstablishmentAsync(establishmentModel)
            .Returns(expectedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetOrCreateEstablishmentAsync(establishmentModel);

        // Assert
        Assert.Same(expectedEstablishment, result);
    }

    [Fact]
    public async Task GetOrCreateSchoolAsync_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        var expectedSchool = new SqlEstablishmentDtoBuilder().WithId(701).Build();
        _establishmentWorkflow
            .GetOrCreateSchoolAsync("100701", "Northfield Primary")
            .Returns(expectedSchool);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetOrCreateSchoolAsync("100701", "Northfield Primary");

        // Assert
        Assert.Same(expectedSchool, result);
    }

    [Fact]
    public async Task GetOrCreateSchoolsAsync_WhenGivenSchools_ReturnsOneEstablishmentPerSchoolInOrder()
    {
        // Arrange
        var firstSchool = new SelectedEstablishmentDataModelBuilder()
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        var secondSchool = new SelectedEstablishmentDataModelBuilder()
            .WithUrn("100702")
            .WithName("Southfield Primary")
            .Build();
        _establishmentWorkflow
            .GetOrCreateSchoolAsync("100701", "Northfield Primary")
            .Returns(new SqlEstablishmentDtoBuilder().WithId(701).Build());
        _establishmentWorkflow
            .GetOrCreateSchoolAsync("100702", "Southfield Primary")
            .Returns(new SqlEstablishmentDtoBuilder().WithId(702).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetOrCreateSchoolsAsync([firstSchool, secondSchool]);

        // Assert
        Assert.Equal([701, 702], result.Select(establishment => establishment.Id).ToArray());
    }

    [Fact]
    public async Task GetEstablishmentsByReferencesAsync_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        IEnumerable<SqlEstablishmentDto> expectedEstablishments =
        [
            new SqlEstablishmentDtoBuilder().WithId(701).Build(),
        ];
        _establishmentWorkflow
            .GetEstablishmentsByReferencesAsync(
                Arg.Is<IEnumerable<string>>(r => r.Single() == "100701")
            )
            .Returns(expectedEstablishments);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetEstablishmentsByReferencesAsync(["100701"]);

        // Assert
        Assert.Same(expectedEstablishments, result);
    }

    [Fact]
    public async Task GetEstablishmentByReferenceAsync_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        var expectedEstablishment = new SqlEstablishmentDtoBuilder().WithId(701).Build();
        _establishmentWorkflow
            .GetEstablishmentByReferenceAsync("100701")
            .Returns(expectedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetEstablishmentByReferenceAsync("100701");

        // Assert
        Assert.Same(expectedEstablishment, result);
    }

    [Fact]
    public async Task GetEstablishmentByIdAsync_WhenEstablishmentExists_ReturnsIt()
    {
        // Arrange
        var expectedEstablishment = new SqlEstablishmentDtoBuilder().WithId(701).Build();
        _establishmentWorkflow.GetEstablishmentByIdAsync(701).Returns(expectedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetEstablishmentByIdAsync(701);

        // Assert
        Assert.Same(expectedEstablishment, result);
    }

    [Fact]
    public async Task GetEstablishmentByIdAsync_WhenEstablishmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        _establishmentWorkflow.GetEstablishmentByIdAsync(999).Returns((SqlEstablishmentDto?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            systemUnderTest.GetEstablishmentByIdAsync(999)
        );
        Assert.Equal("Establishment with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task RecordGroupSelection_WhenSelectedEstablishmentExists_RecordsSelectionWithItsId()
    {
        // Arrange
        _establishmentWorkflow
            .GetEstablishmentByIdAsync(701)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(701).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordGroupSelection(42, 301, 701, "100701", "Northfield Primary");

        // Assert
        await _establishmentWorkflow
            .Received(1)
            .RecordGroupSelectionAsync(
                Arg.Is<UserGroupSelectionModel>(selection =>
                    selection.UserId == 42
                    && selection.UserEstablishmentId == 301
                    && selection.SelectedEstablishmentId == 701
                    && selection.SelectedEstablishmentName == "Northfield Primary"
                )
            );
        await _establishmentWorkflow
            .DidNotReceive()
            .GetOrCreateSchoolAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task RecordGroupSelection_WhenSelectedEstablishmentDoesNotExist_CreatesSchoolAndRecordsSelection()
    {
        // Arrange
        _establishmentWorkflow.GetEstablishmentByIdAsync(701).Returns((SqlEstablishmentDto?)null);
        _establishmentWorkflow
            .GetOrCreateSchoolAsync("100701", "Northfield Primary")
            .Returns(new SqlEstablishmentDtoBuilder().WithId(800).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordGroupSelection(42, 301, 701, "100701", "Northfield Primary");

        // Assert
        await _establishmentWorkflow
            .Received(1)
            .RecordGroupSelectionAsync(
                Arg.Is<UserGroupSelectionModel>(selection =>
                    selection.SelectedEstablishmentId == 800
                )
            );
    }
}
