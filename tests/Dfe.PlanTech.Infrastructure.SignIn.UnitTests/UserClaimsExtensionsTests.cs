using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Infrastructure.SignIns.Extensions;

namespace Dfe.PlanTech.Infrastructure.SignIns.UnitTests;

public class UserClaimsExtensionsTests
{
    [Fact]
    public void GetUserId_Should_Return_UserId_When_ClaimsPrincipal_Exists()
    {
        string expectedUserId = "TestingName";

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimConstants.NameIdentifier, expectedUserId) });
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var userId = UserClaimsExtensions.GetUserId(claimsPrincipal.Claims);

        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void GetUserId_Should_Throw_When_Claim_Is_Missing()
    {
        var identity = new ClaimsIdentity(Array.Empty<Claim>());
        var claimsPrincipal = new ClaimsPrincipal(identity);

        Assert.ThrowsAny<Exception>(() => UserClaimsExtensions.GetUserId(claimsPrincipal.Claims));
    }


    [Fact]
    public void GetUserId_Should_Throw_When_ClaimsPrincipal_Is_Null()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => UserClaimsExtensions.GetUserId(null!));
    }

    [Fact]
    public void GetOrganisation_Should_Throw_When_ClaimsPrincipal_Is_Null()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => UserClaimsExtensions.GetOrganisation(null!));
    }

    [Fact]
    public void GetOrganisation_Should_ReturnNull_When_Org_Claim_Is_Missing()
    {
        var identity = new ClaimsIdentity(Array.Empty<Claim>());
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var organisation = UserClaimsExtensions.GetOrganisation(claimsPrincipal.Claims);

        Assert.Null(organisation);
    }

    [Fact]
    public void GetOrganisation_Should_Throw_When_OrgClaim_Is_NotJson()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimConstants.Organisation, "not a real claim") });
        var claimsPrincipal = new ClaimsPrincipal(identity);

        Assert.ThrowsAny<Exception>(() => UserClaimsExtensions.GetOrganisation(claimsPrincipal.Claims));
    }

    [Fact]
    public void GetOrganisation_Should_ReturnNull_When_Organisation_Has_No_Id()
    {
        var organisation = new Organisation()
        {
            Id = Guid.Empty,
        };

        var organisationJson = JsonSerializer.Serialize(organisation);

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimConstants.Organisation, organisationJson) });
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var foundOrganisation = UserClaimsExtensions.GetOrganisation(claimsPrincipal.Claims);

        Assert.Null(foundOrganisation);
    }

    [Fact]
    public void GetOrganisation_Should_Return_Organisation_When_Exists()
    {
        var organisation = new Organisation()
        {
            Id = Guid.NewGuid()
        };

        var organisationJson = JsonSerializer.Serialize(organisation);

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimConstants.Organisation, organisationJson) });
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var foundOrganisation = UserClaimsExtensions.GetOrganisation(claimsPrincipal.Claims);

        Assert.NotNull(foundOrganisation);
        Assert.Equal(organisation.Id, foundOrganisation.Id);
    }
}
