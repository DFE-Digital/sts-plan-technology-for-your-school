using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Tests.Shared.Fakes;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers.CurrentUserProviderTests;

public class GroupSchoolsTests : CurrentUserProviderTestBase
{
    [Fact]
    public async Task GetGroupSchoolsAsync_WhenUserIsNotAuthenticated_ReturnsEmptyList()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(isAuthenticated: false);

        // Act
        var result = await systemUnderTest.GetGroupSchoolsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetGroupSchoolsAsync_WhenUserCannotSelectSchools_ThrowsInvalidOperationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.GetGroupSchoolsAsync()
        );
        Assert.Equal("Attempted to get group schools for a non-group user", exception.Message);
    }

    [Fact]
    public async Task GetGroupSchoolsAsync_WhenGroupIsInSession_ReturnsItWithoutQueryingGias()
    {
        // Arrange
        var session = new FakeSession();
        session.SetGroupSchools([
            new SelectedEstablishmentDataModelBuilder().WithUrn("100701").Build(),
        ]);
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        var result = await systemUnderTest.GetGroupSchoolsAsync();

        // Assert
        Assert.Equal(["100701"], result.Select(school => school.Urn).ToArray());
        await _giasService.DidNotReceive().GetSchoolsForMatAsync(Arg.Any<EstablishmentModel>());
    }

    [Fact]
    public async Task GetGroupSchoolsAsync_WhenGroupIsNotInSession_LoadsItFromGiasAndStoresIt()
    {
        // Arrange
        var session = new FakeSession();
        var school = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(701)
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        _giasService.GetSchoolsForMatAsync(Arg.Any<EstablishmentModel>()).Returns([school]);
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([
                new SqlEstablishmentDtoBuilder().WithId(701).WithEstablishmentRef("100701").Build(),
            ]);
        _establishmentService
            .GetOrCreateSchoolsAsync(Arg.Any<IEnumerable<SelectedEstablishmentDataModel>>())
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().WithUid("42").Build(),
            session
        );

        // Act
        var result = await systemUnderTest.GetGroupSchoolsAsync();

        // Assert
        Assert.Equal(["100701"], result.Select(s => s.Urn).ToArray());
        Assert.Equal(["100701"], session.GetGroupSchools().Select(s => s.Urn).ToArray());
    }

    [Fact]
    public async Task GetGroupSchoolsAsync_WhenGroupIsNotInSessionAndOrganisationHasNoUid_ReturnsEmptyList()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder()
                .AsMultiAcademyTrust()
                .WithUid(null)
                .WithUrn("100301")
                .Build()
        );

        // Act
        var result = await systemUnderTest.GetGroupSchoolsAsync();

        // Assert
        Assert.Empty(result);
        await _giasService.DidNotReceive().GetSchoolsForMatAsync(Arg.Any<EstablishmentModel>());
    }

    [Theory]
    [InlineData("", "Northfield Primary")]
    [InlineData("100701", " ")]
    public async Task SetGroupSchoolsAsync_WhenASchoolHasNoUrnOrName_ThrowsInvalidDataException(
        string urn,
        string name
    )
    {
        // Arrange
        var session = new FakeSession();
        var systemUnderTest = CreateSystemUnderTest(session: session);
        var invalidSchool = new SelectedEstablishmentDataModelBuilder()
            .WithUrn(urn)
            .WithName(name)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.SetGroupSchoolsAsync([invalidSchool])
        );
        Assert.Equal("All group schools must have a URN and a name", exception.Message);
        Assert.Empty(session.GetGroupSchools());
    }

    [Fact]
    public async Task SetGroupSchoolsAsync_WhenSchoolsAlreadyExistInDatabase_SavesSchoolsWithoutWarning()
    {
        // Arrange
        var expectedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(701)
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        var session = new FakeSession();
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([
                new SqlEstablishmentDtoBuilder().WithId(701).WithEstablishmentRef("100701").Build(),
            ]);
        _establishmentService
            .GetOrCreateSchoolsAsync(Arg.Any<IEnumerable<SelectedEstablishmentDataModel>>())
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest(session: session);

        // Act
        await systemUnderTest.SetGroupSchoolsAsync([expectedSchool]);

        // Assert
        var school = Assert.Single(session.GetGroupSchools());
        Assert.Equal(701, school.EstablishmentId);
        Assert.Equal("100701", school.Urn);
        Assert.Equal("Northfield Primary", school.Name);
        Assert.Empty(_logger.Collector.GetSnapshot());
    }

    [Fact]
    public async Task SetGroupSchoolsAsync_WhenSomeSchoolsAreMissingFromDatabase_CreatesThemAndSavesAllSchools()
    {
        // Arrange
        var existingSchool = new SelectedEstablishmentDataModelBuilder().WithUrn("100701").Build();
        var missingSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(702)
            .WithUrn("100702")
            .WithName("Southfield Primary")
            .Build();
        var session = new FakeSession();
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([
                new SqlEstablishmentDtoBuilder().WithId(701).WithEstablishmentRef("100701").Build(),
            ]);
        _establishmentService
            .GetOrCreateSchoolsAsync(
                Arg.Is<IEnumerable<SelectedEstablishmentDataModel>>(schools =>
                    schools.Select(school => school.Urn).SequenceEqual(new[] { "100702" })
                )
            )
            .Returns([
                new SqlEstablishmentDtoBuilder().WithId(702).WithEstablishmentRef("100702").Build(),
            ]);
        var systemUnderTest = CreateSystemUnderTest(session: session);

        // Act
        await systemUnderTest.SetGroupSchoolsAsync([existingSchool, missingSchool]);

        // Assert
        Assert.Equal(
            ["100701", "100702"],
            session.GetGroupSchools().Select(school => school.Urn).ToArray()
        );
    }

    [Fact]
    public async Task SetGroupSchoolsAsync_WhenDatabaseCannotProvideEverySchool_ThrowsInvalidDataException()
    {
        // Arrange
        var session = new FakeSession();
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([]);
        _establishmentService
            .GetOrCreateSchoolsAsync(Arg.Any<IEnumerable<SelectedEstablishmentDataModel>>())
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest(session: session);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.SetGroupSchoolsAsync([
                new SelectedEstablishmentDataModelBuilder().Build(),
            ])
        );
        Assert.Equal("One or more establishments do not exist in the database.", exception.Message);
        Assert.Empty(session.GetGroupSchools());
    }
}
