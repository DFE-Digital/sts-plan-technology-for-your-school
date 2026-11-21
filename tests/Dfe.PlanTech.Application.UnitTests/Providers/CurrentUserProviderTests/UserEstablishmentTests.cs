using System.Security.Authentication;
using System.Security.Claims;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Tests.Shared.Fakes;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers.CurrentUserProviderTests;

public class UserEstablishmentTests : CurrentUserProviderTestBase
{
    [Fact]
    public async Task GetUserEstablishmentAsync_WhenEstablishmentIsInSession_ReturnsItWithoutQueryingDatabase()
    {
        // Arrange
        var session = new FakeSession();
        session.SetUserEstablishment(
            new SqlEstablishmentDtoBuilder()
                .WithId(301)
                .WithOrgName("Northfield Academy Trust")
                .Build()
        );
        var systemUnderTest = CreateSystemUnderTest(session: session);

        // Act
        var result = await systemUnderTest.GetUserEstablishmentAsync();

        // Assert
        Assert.Equal(301, result.Id);
        Assert.Equal("Northfield Academy Trust", result.OrgName);
        await _establishmentService.DidNotReceive().GetEstablishmentByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task GetUserEstablishmentAsync_WhenSessionIsEmpty_LoadsEstablishmentFromClaimIdAndStoresIt()
    {
        // Arrange
        var session = new FakeSession();
        _establishmentService
            .GetEstablishmentByIdAsync(301)
            .Returns(
                new SqlEstablishmentDtoBuilder()
                    .WithId(301)
                    .WithOrgName("Northfield Academy Trust")
                    .Build()
            );
        var systemUnderTest = CreateSystemUnderTest(
            session: session,
            claims: [new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, "301")]
        );

        // Act
        var result = await systemUnderTest.GetUserEstablishmentAsync();

        // Assert
        Assert.Equal(301, result.Id);
        Assert.Equal(301, session.GetUserEstablishment()!.Id);
    }

    [Fact]
    public async Task GetUserEstablishmentAsync_WhenSessionIsEmptyAndClaimIsMissing_ThrowsAuthenticationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            systemUnderTest.GetUserEstablishmentAsync()
        );
        Assert.Equal("User establishment ID missing from claims", exception.Message);
    }

    [Fact]
    public void SetUserEstablishment_WhenCalled_StoresEstablishmentInSession()
    {
        // Arrange
        var session = new FakeSession();
        var systemUnderTest = CreateSystemUnderTest(session: session);
        var establishment = new SqlEstablishmentDtoBuilder().WithId(301).Build();

        // Act
        systemUnderTest.SetUserEstablishment(establishment);

        // Assert
        Assert.Equivalent(establishment, session.GetUserEstablishment());
    }

    [Fact]
    public void SetUserEstablishment_WhenThereIsNoHttpContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        _contextAccessor.HttpContext = null;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            systemUnderTest.SetUserEstablishment(new SqlEstablishmentDtoBuilder().Build())
        );
        Assert.Equal("Cannot access the current session.", exception.Message);
    }
}
