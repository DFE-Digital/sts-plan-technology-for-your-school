using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Core.UnitTests.Helpers;

public class UserClaimsHelperTests
{
    [Fact]
    public void GetDsiReference_Should_Return_UserId_When_ClaimsPrincipal_Exists()
    {
        string expectedUserId = "TestingName";

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimConstants.NameIdentifier, expectedUserId) }
        );
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var userId = claimsPrincipal.Claims.GetDsiReference();

        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void GetDsiReference_Should_Throw_When_ClaimsPrincipal_Is_Null()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => UserClaimsHelper.GetDsiReference(null!));
    }

    [Fact]
    public void GetOrganisation_Should_Throw_When_ClaimsPrincipal_Is_Null()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => UserClaimsHelper.GetOrganisation(null!));
    }

    [Fact]
    public void GetOrganisation_Should_ReturnNull_When_Org_Claim_Is_Missing()
    {
        var identity = new ClaimsIdentity(Array.Empty<Claim>());
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var organisation = claimsPrincipal.Claims.GetOrganisation();

        Assert.Null(organisation);
    }

    [Fact]
    public void GetOrganisation_Should_Throw_When_OrgClaim_Is_NotJson()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimConstants.Organisation, "not a real claim") }
        );
        var claimsPrincipal = new ClaimsPrincipal(identity);

        Assert.ThrowsAny<Exception>(() => claimsPrincipal.Claims.GetOrganisation());
    }

    [Fact]
    public void GetOrganisation_Should_ReturnNull_When_Organisation_Has_No_Id()
    {
        var organisation = new EstablishmentModel()
        {
            Id = Guid.Empty,
            Urn = "testUrn",
            Ukprn = "testUkPrn",
        };

        var organisationJson = JsonSerializer.Serialize(organisation);

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimConstants.Organisation, organisationJson) }
        );
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var foundOrganisation = claimsPrincipal.Claims.GetOrganisation();

        Assert.Null(foundOrganisation);
    }

    [Fact]
    public void GetOrganisation_Should_Return_Organisation_When_Exists()
    {
        var organisation = new EstablishmentModel()
        {
            Id = Guid.NewGuid(),
            Urn = "testUrn",
            Ukprn = "testUkPrn",
        };

        var organisationJson = JsonSerializer.Serialize(organisation);

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimConstants.Organisation, organisationJson) }
        );
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var foundOrganisation = claimsPrincipal.Claims.GetOrganisation();

        Assert.NotNull(foundOrganisation);
        Assert.Equal(organisation.Id, foundOrganisation.Id);
    }
}
