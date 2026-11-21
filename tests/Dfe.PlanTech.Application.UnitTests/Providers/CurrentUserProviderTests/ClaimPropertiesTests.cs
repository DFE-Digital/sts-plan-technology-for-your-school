using System.Security.Authentication;
using System.Security.Claims;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Tests.Shared.Builders.Models;

namespace Dfe.PlanTech.Application.UnitTests.Providers.CurrentUserProviderTests;

public class ClaimPropertiesTests : CurrentUserProviderTestBase
{
    private const string SessionIdValue = "5f1c5c1e-3d8a-4a0e-9d5b-0b1c2d3e4f50";

    [Fact]
    public void DsiReference_WhenNameIdentifierClaimIsPresent_ReturnsClaimValue()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            claims: [new Claim(ClaimConstants.NameIdentifier, "dsi-user-42")]
        );

        // Act
        var result = systemUnderTest.DsiReference;

        // Assert
        Assert.Equal("dsi-user-42", result);
    }

    [Fact]
    public void DsiReference_WhenNameIdentifierClaimIsMissing_ThrowsAuthenticationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = Assert.Throws<AuthenticationException>(() => systemUnderTest.DsiReference);
        Assert.Equal("User is not authenticated", exception.Message);
    }

    [Fact]
    public void FocusedEstablishmentId_WhenClaimIsNumeric_ReturnsId()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            claims: [new Claim(ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID, "701")]
        );

        // Act
        var result = systemUnderTest.FocusedEstablishmentId;

        // Assert
        Assert.Equal(701, result);
    }

    [Fact]
    public void FocusedEstablishmentId_WhenClaimIsMissing_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = systemUnderTest.FocusedEstablishmentId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TrustEstablishmentId_WhenClaimIsNumeric_ReturnsId()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            claims: [new Claim(ClaimConstants.DB_TRUST_ESTABLISHMENT_ID, "301")]
        );

        // Act
        var result = systemUnderTest.TrustEstablishmentId;

        // Assert
        Assert.Equal(301, result);
    }

    [Fact]
    public void TrustEstablishmentId_WhenClaimIsNotNumeric_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            claims: [new Claim(ClaimConstants.DB_TRUST_ESTABLISHMENT_ID, "trust-id")]
        );

        // Act
        var result = systemUnderTest.TrustEstablishmentId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SessionId_WhenClaimIsAGuid_ReturnsGuid()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            claims: [new Claim(ClaimConstants.SessionId, SessionIdValue)]
        );

        // Act
        var result = systemUnderTest.SessionId;

        // Assert
        Assert.Equal(Guid.Parse(SessionIdValue), result);
    }

    [Fact]
    public void SessionId_WhenClaimIsMissing_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = systemUnderTest.SessionId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UserEstablishmentId_WhenClaimIsNumeric_ReturnsId()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            claims: [new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, "301")]
        );

        // Act
        var result = systemUnderTest.UserEstablishmentId;

        // Assert
        Assert.Equal(301, result);
    }

    [Fact]
    public void UserEstablishmentId_WhenClaimIsMissing_ThrowsAuthenticationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = Assert.Throws<AuthenticationException>(() =>
            systemUnderTest.UserEstablishmentId
        );
        Assert.Equal("User establishment ID missing from claims", exception.Message);
    }

    [Fact]
    public void UserId_WhenClaimIsNumeric_ReturnsId()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            claims: [new Claim(ClaimConstants.DB_USER_ID, "42")]
        );

        // Act
        var result = systemUnderTest.UserId;

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void UserId_WhenClaimIsMissing_ThrowsAuthenticationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = Assert.Throws<AuthenticationException>(() => systemUnderTest.UserId);
        Assert.Equal("User ID missing from claims", exception.Message);
    }

    [Fact]
    public void DsiOrganisation_WhenOrganisationClaimIsPresent_ReturnsOrganisation()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .AsMultiAcademyTrust()
            .WithName("Northfield Academy Trust")
            .WithUid("42")
            .Build();
        var systemUnderTest = CreateSystemUnderTest(organisation);

        // Act
        var result = systemUnderTest.DsiOrganisation;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Northfield Academy Trust", result.Name);
        Assert.Equal("42", result.Uid);
        Assert.Equal(DsiConstants.MatOrganisationCategoryId, result.Category!.Id);
    }

    [Fact]
    public void DsiOrganisation_WhenOrganisationClaimIsMissing_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = systemUnderTest.DsiOrganisation;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DsiOrganisation_WhenThereIsNoHttpContext_ReturnsNull()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        _contextAccessor.HttpContext = null;

        // Act
        var result = systemUnderTest.DsiOrganisation;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IsAuthenticated_WhenIdentityIsAuthenticated_ReturnsTrue()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(isAuthenticated: true);

        // Act
        var result = systemUnderTest.IsAuthenticated;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAuthenticated_WhenIdentityIsNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(isAuthenticated: false);

        // Act
        var result = systemUnderTest.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_WhenThereIsNoHttpContext_ReturnsFalse()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        _contextAccessor.HttpContext = null;

        // Act
        var result = systemUnderTest.IsAuthenticated;

        // Assert
        Assert.False(result);
    }
}
