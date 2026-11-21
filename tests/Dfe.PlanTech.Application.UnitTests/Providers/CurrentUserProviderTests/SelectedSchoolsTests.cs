using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Tests.Shared.Fakes;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers.CurrentUserProviderTests;

public class SelectedSchoolsTests : CurrentUserProviderTestBase
{
    [Fact]
    public void ClearSelectedSchools_WhenSchoolsAreSelected_RemovesThem()
    {
        // Arrange
        var session = new FakeSession();
        session.SetSelectedEstablishments(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(session: session);

        // Act
        systemUnderTest.ClearSelectedSchools();

        // Assert
        Assert.Empty(session.GetSelectedSchools());
    }

    [Fact]
    public void GetSelectedSchools_WhenUserIsNotAuthenticated_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(isAuthenticated: false);

        // Act
        var result = systemUnderTest.GetSelectedSchools();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetSelectedSchools_WhenUserCannotSelectSchools_ThrowsInvalidOperationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build()
        );

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            systemUnderTest.GetSelectedSchools()
        );
        Assert.Equal("Attempted to get selected schools for a non-group user", exception.Message);
    }

    [Fact]
    public void GetSelectedSchools_WhenSchoolsAreSelected_ReturnsThem()
    {
        // Arrange
        var session = new FakeSession();
        session.SetSelectedEstablishments(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        var result = systemUnderTest.GetSelectedSchools();

        // Assert
        var school = Assert.Single(result!);
        Assert.Equal(701, school.EstablishmentId);
        Assert.Equal("100701", school.Urn);
        Assert.Equal("Northfield Primary", school.Name);
    }

    [Fact]
    public void GetSelectedSchools_WhenNoSchoolsAreSelected_ReturnsEmptyList()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build()
        );

        // Act
        var result = systemUnderTest.GetSelectedSchools();

        // Assert
        Assert.Empty(result!);
    }

    [Fact]
    public void GetSelectedSchoolEstablishmentIds_WhenSchoolsAreSelected_ReturnsTheirEstablishmentIds()
    {
        // Arrange
        var session = new FakeSession();
        session.SetSelectedSchools([
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(701)
                .WithUrn("100701")
                .Build(),
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(702)
                .WithUrn("100702")
                .Build(),
        ]);
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        var result = systemUnderTest.GetSelectedSchoolEstablishmentIds();

        // Assert
        Assert.Equal([701, 702], result);
    }

    [Fact]
    public void GetSelectedSchoolEstablishmentIds_WhenUserIsNotAuthenticated_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(isAuthenticated: false);

        // Act
        var result = systemUnderTest.GetSelectedSchoolEstablishmentIds();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetSelectedSchoolNames_WhenSchoolsAreSelected_ReturnsTheirNames()
    {
        // Arrange
        var session = new FakeSession();
        session.SetSelectedSchools([
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(701)
                .WithName("Northfield Primary")
                .Build(),
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(702)
                .WithName("Southfield Primary")
                .Build(),
        ]);
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        var result = systemUnderTest.GetSelectedSchoolNames();

        // Assert
        Assert.Equal(["Northfield Primary", "Southfield Primary"], result);
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenUserCannotSelectSchools_ThrowsInvalidOperationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.SetSelectedSchoolsAsync(
                new SelectedEstablishmentDataModelBuilder().Build()
            )
        );
        Assert.Equal("User cannot set selected establishments", exception.Message);
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenSelectionIsEmpty_ClearsSelectionWithoutValidating()
    {
        // Arrange
        var session = new FakeSession();
        session.SetSelectedEstablishments(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        await systemUnderTest.SetSelectedSchoolsAsync([]);

        // Assert
        Assert.Empty(session.GetSelectedSchools());
        await _establishmentService.DidNotReceive().GetEstablishmentByIdAsync(Arg.Any<int>());
        await _giasService.DidNotReceive().GetSchoolsForMatAsync(Arg.Any<EstablishmentModel>());
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenEverySchoolIsInTheGroup_StoresSelection()
    {
        // Arrange
        var session = new FakeSession();
        session.SetUserEstablishment(
            new SqlEstablishmentDtoBuilder().WithGroupUid("group-42").Build()
        );
        session.SetGroupSchools([
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(701)
                .WithUrn("100701")
                .Build(),
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(702)
                .WithUrn("100702")
                .Build(),
        ]);
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        await systemUnderTest.SetSelectedSchoolsAsync(
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(702)
                .WithUrn("100702")
                .Build()
        );

        // Assert
        Assert.Equal(
            ["100702"],
            session.GetSelectedSchools().Select(school => school.Urn).ToArray()
        );
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenSelectionIsOutsideTheGroup_ClearsAndThrows()
    {
        // Arrange
        var session = new FakeSession();
        session.SetUserEstablishment(new SqlEstablishmentDtoBuilder().Build());
        session.SetGroupSchools([
            new SelectedEstablishmentDataModelBuilder().WithUrn("100701").Build(),
        ]);
        session.SetSelectedEstablishments(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );
        var invalidSelection = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(702)
            .WithUrn("100702")
            .Build();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.SetSelectedSchoolsAsync(invalidSelection)
        );

        // Assert
        Assert.Equal(
            "One or more selected establishments are not in the user's group.",
            exception.Message
        );
        Assert.Empty(session.GetSelectedSchools());
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains("100702", logRecord.Message);
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenGroupUidIsUnavailable_LogsWarningClearsAndThrows()
    {
        // Arrange
        var session = new FakeSession();
        session.SetUserEstablishment(new SqlEstablishmentDtoBuilder().WithGroupUid(null).Build());
        session.SetSelectedEstablishments(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().WithGroupUid(null).Build(),
            session
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.SetSelectedSchoolsAsync(
                new SelectedEstablishmentDataModelBuilder().WithUrn("100701").Build()
            )
        );
        Assert.Empty(session.GetSelectedSchools());
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains("Group UID is empty", logRecord.Message);
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenGroupIsNotInSession_LoadsItFromGiasBeforeValidating()
    {
        // Arrange
        var firstGroupSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(701)
            .WithUrn("100701")
            .Build();
        var selectedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(702)
            .WithUrn("100702")
            .Build();
        var session = new FakeSession();
        session.SetUserEstablishment(new SqlEstablishmentDtoBuilder().Build());
        _giasService
            .GetSchoolsForMatAsync(Arg.Any<EstablishmentModel>())
            .Returns([firstGroupSchool, selectedSchool]);
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([
                new SqlEstablishmentDtoBuilder().WithId(701).WithEstablishmentRef("100701").Build(),
                new SqlEstablishmentDtoBuilder().WithId(702).WithEstablishmentRef("100702").Build(),
            ]);
        _establishmentService
            .GetOrCreateSchoolsAsync(Arg.Any<IEnumerable<SelectedEstablishmentDataModel>>())
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().WithUid("42").Build(),
            session
        );

        // Act
        await systemUnderTest.SetSelectedSchoolsAsync(selectedSchool);

        // Assert
        Assert.Equal(
            ["100701", "100702"],
            session.GetGroupSchools().Select(school => school.Urn).ToArray()
        );
        Assert.Equal(
            ["100702"],
            session.GetSelectedSchools().Select(school => school.Urn).ToArray()
        );
    }
}
