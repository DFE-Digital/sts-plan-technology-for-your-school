using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class GiasWorkflowTests
{
    private readonly IGiasRepository _giasRepository = Substitute.For<IGiasRepository>();

    private GiasWorkflow CreateSystemUnderTest() => new(_giasRepository);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<GiasWorkflow>(_giasRepository);
    }

    [Fact]
    public async Task GetSchoolByUrn_WhenUrnIsNotNumeric_ThrowsInvalidDataException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.GetSchoolByUrn("school-urn")
        );
        Assert.Equal("Provided string schoolUrn cannot be parsed as a URN", exception.Message);
        await _giasRepository.DidNotReceive().GetSchoolByUrn(Arg.Any<int>());
    }

    [Fact]
    public async Task GetSchoolByUrn_WhenSchoolExists_ReturnsMappedSchool()
    {
        // Arrange
        var school = new GiasEstablishmentEntityBuilder()
            .WithUrn(100701)
            .WithEstablishmentName("Northfield Primary")
            .WithTypeOfEstablishment(34, "Academy converter")
            .Build();
        _giasRepository.GetSchoolByUrn(100701).Returns(school);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSchoolByUrn("100701");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100701, result.Urn);
        Assert.Equal("Northfield Primary", result.EstablishmentName);
        Assert.Equal("Academy converter", result.TypeOfEstablishmentName);
    }

    [Fact]
    public async Task GetSchoolByUrn_WhenSchoolDoesNotExist_ReturnsNull()
    {
        // Arrange
        _giasRepository.GetSchoolByUrn(100702).Returns((GiasEstablishmentEntity?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSchoolByUrn("100702");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSchoolsInGroup_WhenGroupHasSchools_ReturnsMappedSchools()
    {
        // Arrange
        _giasRepository
            .GetSchoolsForGroupAsync(42)
            .Returns([
                new GiasEstablishmentEntityBuilder().WithUrn(100701).Build(),
                new GiasEstablishmentEntityBuilder().WithUrn(100702).Build(),
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSchoolsInGroup(42);

        // Assert
        Assert.Equal([100701, 100702], result.Select(school => school.Urn).ToArray());
    }

    [Fact]
    public async Task GetSingleAcademySchool_WhenSchoolExists_ReturnsMappedSchool()
    {
        // Arrange
        var school = new GiasEstablishmentEntityBuilder()
            .WithUrn(100703)
            .WithEstablishmentName("Eastfield Primary")
            .Build();
        _giasRepository.GetSingleAcademySchool(42).Returns(school);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSingleAcademySchool(42);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100703, result.Urn);
        Assert.Equal("Eastfield Primary", result.EstablishmentName);
    }

    [Fact]
    public async Task GetSingleAcademySchool_WhenGroupHasNoSchool_ReturnsNull()
    {
        // Arrange
        _giasRepository.GetSingleAcademySchool(42).Returns((GiasEstablishmentEntity?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSingleAcademySchool(42);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HasAllSchoolsInGroup_WhenEveryUrnIsInTheGroup_ReturnsTrue()
    {
        // Arrange
        _giasRepository
            .GetSchoolsForGroupAsync(42)
            .Returns([
                new GiasEstablishmentEntityBuilder().WithUrn(100701).Build(),
                new GiasEstablishmentEntityBuilder().WithUrn(100702).Build(),
                new GiasEstablishmentEntityBuilder().WithUrn(100703).Build(),
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.HasAllSchoolsInGroup(42, [100701, 100703]);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAllSchoolsInGroup_WhenAnyUrnIsNotInTheGroup_ReturnsFalse()
    {
        // Arrange
        _giasRepository
            .GetSchoolsForGroupAsync(42)
            .Returns([new GiasEstablishmentEntityBuilder().WithUrn(100701).Build()]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.HasAllSchoolsInGroup(42, [100701, 100999]);

        // Assert
        Assert.False(result);
    }
}
