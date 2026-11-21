using System.Security.Claims;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Tests.Shared.Fakes;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers.CurrentUserProviderTests;

public class FocusedEstablishmentTests : CurrentUserProviderTestBase
{
    [Fact]
    public async Task GetFocusedEstablishmentAsync_WhenUserIsNotAuthenticated_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(isAuthenticated: false);

        // Act
        var result = await systemUnderTest.GetFocusedEstablishmentAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFocusedEstablishmentAsync_WhenGroupUserHasNotFocusedASchool_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build()
        );

        // Act
        var result = await systemUnderTest.GetFocusedEstablishmentAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFocusedEstablishmentAsync_WhenGroupUserHasFocusedASchool_ReturnsIt()
    {
        // Arrange
        var session = new FakeSession();
        session.SetFocusedEstablishment(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        var result = await systemUnderTest.GetFocusedEstablishmentAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(701, result.EstablishmentId);
        Assert.Equal("100701", result.Urn);
        Assert.Equal("Northfield Primary", result.Name);
    }

    [Fact]
    public async Task GetFocusedEstablishmentAsync_WhenSchoolIsAlreadyInSession_DoesNotQueryDatabase()
    {
        // Arrange
        var session = new FakeSession();
        session.SetFocusedEstablishment(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build(),
            session,
            [new Claim(ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID, "701")]
        );

        // Act
        var result = await systemUnderTest.GetFocusedEstablishmentAsync();

        // Assert
        Assert.Equal(701, result!.EstablishmentId);
        await _establishmentService.DidNotReceive().GetEstablishmentByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task GetFocusedEstablishmentAsync_WhenSessionIsEmptyButClaimHasSchool_LoadsAndStoresSchool()
    {
        // Arrange
        var session = new FakeSession();
        _establishmentService
            .GetEstablishmentByIdAsync(701)
            .Returns(
                new SqlEstablishmentDtoBuilder()
                    .WithId(701)
                    .WithEstablishmentRef("100701")
                    .WithOrgName("Northfield Primary")
                    .Build()
            );
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build(),
            session,
            [new Claim(ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID, "701")]
        );

        // Act
        var result = await systemUnderTest.GetFocusedEstablishmentAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(701, result.EstablishmentId);
        Assert.Equal("100701", result.Urn);
        Assert.Equal("Northfield Primary", result.Name);
        Assert.Equal(701, session.GetFocusedEstablishment()!.EstablishmentId);
    }

    [Fact]
    public async Task GetFocusedEstablishmentAsync_WhenLoadedSchoolHasNoName_UsesUnknownName()
    {
        // Arrange
        _establishmentService
            .GetEstablishmentByIdAsync(701)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(701).WithOrgName(null).Build());
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build(),
            claims: [new Claim(ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID, "701")]
        );

        // Act
        var result = await systemUnderTest.GetFocusedEstablishmentAsync();

        // Assert
        Assert.Equal("Unknown name", result!.Name);
    }

    [Fact]
    public async Task GetFocusedEstablishmentAsync_WhenSchoolUserHasNoFocusedSchool_ThrowsInvalidDataException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.GetFocusedEstablishmentAsync()
        );
        Assert.Equal("There is no focused school present in the session", exception.Message);
    }

    [Fact]
    public async Task SetFocusedEstablishmentAsync_WhenUserCannotSelectSchools_ThrowsInvalidOperationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.SetFocusedEstablishmentAsync("100701", "Northfield Primary")
        );
        Assert.Equal("Non-group user attempted to set their focused school", exception.Message);
    }

    [Theory]
    [InlineData("", "Northfield Primary")]
    [InlineData("100701", "")]
    public async Task SetFocusedEstablishmentAsync_WhenUrnOrNameIsEmpty_ThrowsInvalidDataException(
        string urn,
        string name
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build()
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.SetFocusedEstablishmentAsync(urn, name)
        );
        Assert.Equal("No URN/school name set for selection.", exception.Message);
    }

    [Fact]
    public async Task SetFocusedEstablishmentAsync_WhenSchoolIsNotInGroup_ThrowsInvalidOperationException()
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

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.SetFocusedEstablishmentAsync("100999", "Westfield Primary")
        );
        Assert.Equal(
            "User attempted to set a focused school that is not in their group",
            exception.Message
        );
        Assert.Null(session.GetFocusedEstablishment());
    }

    [Fact]
    public async Task SetFocusedEstablishmentAsync_WhenSchoolIsInGroup_StoresFocusedSchool()
    {
        // Arrange
        var session = new FakeSession();
        session.SetGroupSchools([
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(701)
                .WithUrn("100701")
                .WithName("Northfield Primary")
                .Build(),
        ]);
        _establishmentService
            .GetOrCreateSchoolAsync("100701", "Northfield Primary")
            .Returns(
                new SqlEstablishmentDtoBuilder()
                    .WithId(701)
                    .WithEstablishmentRef("100701")
                    .WithOrgName("Northfield Primary")
                    .Build()
            );
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        await systemUnderTest.SetFocusedEstablishmentAsync("100701", "Northfield Primary");

        // Assert
        var focusedSchool = session.GetFocusedEstablishment();
        Assert.NotNull(focusedSchool);
        Assert.Equal(701, focusedSchool.EstablishmentId);
        Assert.Equal("100701", focusedSchool.Urn);
        Assert.Equal("Northfield Primary", focusedSchool.Name);
    }

    [Fact]
    public void ClearFocusedEstablishment_WhenUserCanSelectSchools_RemovesFocusedSchool()
    {
        // Arrange
        var session = new FakeSession();
        session.SetFocusedEstablishment(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsMultiAcademyTrust().Build(),
            session
        );

        // Act
        systemUnderTest.ClearFocusedEstablishment();

        // Assert
        Assert.Null(session.GetFocusedEstablishment());
    }

    [Fact]
    public void ClearFocusedEstablishment_WhenUserCannotSelectSchools_ThrowsInvalidOperationException()
    {
        // Arrange
        var session = new FakeSession();
        session.SetFocusedEstablishment(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder().AsSchool().Build(),
            session
        );

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            systemUnderTest.ClearFocusedEstablishment()
        );
        Assert.Equal("Non-group user attempted to clear their focused school", exception.Message);
        Assert.NotNull(session.GetFocusedEstablishment());
    }
}
