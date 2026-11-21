using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class GiasServiceTests
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        Substitute.For<IEstablishmentWorkflow>();
    private readonly IGiasWorkflow _giasWorkflow = Substitute.For<IGiasWorkflow>();

    private GiasService CreateSystemUnderTest() => new(_establishmentWorkflow, _giasWorkflow);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<GiasService>(
            _establishmentWorkflow,
            _giasWorkflow
        );
    }

    [Fact]
    public async Task HasSchoolInGroup_WhenSchoolIsInGroup_ReturnsTrue()
    {
        // Arrange
        _giasWorkflow
            .HasAllSchoolsInGroup(
                42,
                Arg.Is<IEnumerable<int>>(urns => urns.SequenceEqual(new[] { 100701 }))
            )
            .Returns(true);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.HasSchoolInGroup("42", "100701");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAllSchoolsInGroup_WhenGroupUidIsNotNumeric_ThrowsInvalidDataException()
    {
        // Act & Assert
        var systemUnderTest = CreateSystemUnderTest();
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.HasAllSchoolsInGroup("group-uid", ["100701"])
        );
        Assert.Equal("Cannot parse an integer UID from the provided string", exception.Message);
    }

    [Fact]
    public async Task HasAllSchoolsInGroup_WhenAnyUrnIsNotNumeric_ThrowsFormatException()
    {
        // Act & Assert
        var systemUnderTest = CreateSystemUnderTest();
        var exception = await Assert.ThrowsAsync<FormatException>(() =>
            systemUnderTest.HasAllSchoolsInGroup("42", ["100701", "school-urn"])
        );
        Assert.Equal("Cannot parse an integer URN from 'school-urn'", exception.Message);
    }

    [Fact]
    public async Task GetSchoolForSatAsync_WhenOrganisationIsNotSat_ReturnsNull()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(DsiConstants.EstablishmentCategoryId, "Establishment")
            .Build();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSchoolForSatAsync(organisation);

        // Assert
        Assert.Null(result);
        await _giasWorkflow.DidNotReceive().GetSingleAcademySchool(Arg.Any<int>());
    }

    [Fact]
    public async Task GetSchoolForSatAsync_WhenSchoolIsFound_ReturnsSchoolWithDatabaseId()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(DsiConstants.SatOrganisationCategoryId, "Single-academy trust")
            .WithUid("42")
            .WithGroupUid("group-42")
            .Build();
        _giasWorkflow
            .GetSingleAcademySchool(42)
            .Returns(
                new SqlGiasEstablishmentDtoBuilder()
                    .WithUrn(100701)
                    .WithEstablishmentName("Northfield Primary")
                    .WithTypeOfEstablishmentName("Academy converter")
                    .Build()
            );
        _establishmentWorkflow
            .GetOrCreateEstablishmentAsync(
                Arg.Is<EstablishmentModel>(model =>
                    model.Urn == "100701"
                    && model.Name == "Northfield Primary"
                    && model.GroupUid == "group-42"
                    && model.Type!.Name == "Academy converter"
                )
            )
            .Returns(new SqlEstablishmentDtoBuilder().WithId(701).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSchoolForSatAsync(organisation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(701, result.EstablishmentId);
        Assert.Equal("100701", result.Urn);
        Assert.Equal("Northfield Primary", result.Name);
    }

    [Theory]
    [InlineData(DsiConstants.SatOrganisationCategoryId, "SAT")]
    [InlineData(DsiConstants.SSatOrganisationCategoryId, "SSAT")]
    public async Task GetSchoolForSatAsync_WhenSchoolIsNotFound_ThrowsInvalidGiasDataException(
        string categoryId,
        string expectedOrganisationType
    )
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(categoryId, "Trust")
            .WithUid("42")
            .WithGroupUid("group-42")
            .Build();
        _giasWorkflow.GetSingleAcademySchool(42).Returns((SqlGiasEstablishmentDto?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidGiasDataException>(() =>
            systemUnderTest.GetSchoolForSatAsync(organisation)
        );
        Assert.Equal(
            $"School not found for {expectedOrganisationType} with group UID 'group-42'",
            exception.Message
        );
    }

    [Fact]
    public async Task GetSchoolForSatAsync_WhenUidIsNotNumeric_ThrowsWithoutQueryingGias()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(DsiConstants.SatOrganisationCategoryId, "Single-academy trust")
            .WithUid("trust-uid")
            .Build();
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidGiasDataException>(() =>
            systemUnderTest.GetSchoolForSatAsync(organisation)
        );
        await _giasWorkflow.DidNotReceive().GetSingleAcademySchool(Arg.Any<int>());
    }

    [Fact]
    public async Task GetSchoolsForMatAsync_WhenOrganisationCannotSelectSchools_ReturnsEmptyList()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(DsiConstants.SatOrganisationCategoryId, "Single-academy trust")
            .Build();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSchoolsForMatAsync(organisation);

        // Assert
        Assert.Empty(result);
        await _giasWorkflow.DidNotReceive().GetSchoolsInGroup(Arg.Any<int>());
    }

    [Fact]
    public async Task GetSchoolsForMatAsync_WhenUidIsNotNumeric_ThrowsInvalidGiasDataException()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(DsiConstants.MatOrganisationCategoryId, "Multi-academy trust")
            .WithUid("trust-uid")
            .WithGroupUid("group-42")
            .Build();
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidGiasDataException>(() =>
            systemUnderTest.GetSchoolsForMatAsync(organisation)
        );
        Assert.Equal("No schools found for group UID 'group-42'", exception.Message);
    }

    [Fact]
    public async Task GetSchoolsForMatAsync_WhenGroupHasSchools_ReturnsSchoolsWithDatabaseIds()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(DsiConstants.MatOrganisationCategoryId, "Multi-academy trust")
            .WithUid("42")
            .WithGroupUid("group-42")
            .Build();
        _giasWorkflow
            .GetSchoolsInGroup(42)
            .Returns([
                new SqlGiasEstablishmentDtoBuilder().WithUrn(100701).Build(),
                new SqlGiasEstablishmentDtoBuilder().WithUrn(100702).Build(),
            ]);
        _establishmentWorkflow
            .GetOrCreateEstablishmentsAsync(
                Arg.Is<IEnumerable<EstablishmentModel>>(models =>
                    models.Select(model => model.Urn).SequenceEqual(new[] { "100701", "100702" })
                    && models.All(model => model.GroupUid == "group-42")
                )
            )
            .Returns([
                new SqlEstablishmentDtoBuilder()
                    .WithId(701)
                    .WithEstablishmentRef("100701")
                    .WithOrgName("Northfield Primary")
                    .Build(),
                new SqlEstablishmentDtoBuilder()
                    .WithId(702)
                    .WithEstablishmentRef("100702")
                    .WithOrgName("Southfield Primary")
                    .Build(),
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetSchoolsForMatAsync(organisation);

        // Assert
        Assert.Collection(
            result,
            school =>
            {
                Assert.Equal(701, school.EstablishmentId);
                Assert.Equal("100701", school.Urn);
                Assert.Equal("Northfield Primary", school.Name);
            },
            school =>
            {
                Assert.Equal(702, school.EstablishmentId);
                Assert.Equal("100702", school.Urn);
                Assert.Equal("Southfield Primary", school.Name);
            }
        );
    }
}
