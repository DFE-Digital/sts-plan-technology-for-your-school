using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Context;

public class CurrentUserTests
{
    private static (CurrentUser sut, DefaultHttpContext ctx) Build(
        IEnumerable<Claim>? claims = null,
        bool authenticated = true)
    {
        var id = authenticated ? new ClaimsIdentity("test") : new ClaimsIdentity();
        if (claims != null)
        {
            foreach (var c in claims)
                id.AddClaim(c);
        }

        var principal = new ClaimsPrincipal(id);
        var ctx = new DefaultHttpContext();
        ctx.User = principal;

        var accessor = new HttpContextAccessor { HttpContext = ctx };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);
        return (sut, ctx);
    }

    private static Claim BuildClaim(string type, string value) => new(type, value);

    // ---------- ctor guards ----------

    [Fact]
    public void Ctor_Throws_When_ContextAccessor_Null()
    {
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        Assert.Throws<ArgumentNullException>(() => new CurrentUser(null!, establishmentService, logger));
    }

    [Fact]
    public void Ctor_Throws_When_EstablishmentService_Null()
    {
        var accessor = new HttpContextAccessor();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        Assert.Throws<ArgumentNullException>(() => new CurrentUser(accessor, null!, logger));
    }

    // ---------- DsiReference ----------

    [Fact]
    public void DsiReference_Returns_Value_When_Claim_Present()
    {
        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.NameIdentifier, "dsi-123") });
        Assert.Equal("dsi-123", sut.DsiReference);
    }

    [Fact]
    public void DsiReference_Throws_When_Missing()
    {
        var (sut, _) = Build();
        var ex = Assert.Throws<AuthenticationException>(() => _ = sut.DsiReference);
        Assert.Equal("User is not authenticated", ex.Message);
    }

    // ---------- Email ----------

    [Fact]
    public void Email_Returns_Value_When_Claim_Present()
    {
        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.VerifiedEmail, "user@example.com") });
        Assert.Equal("user@example.com", sut.Email);
    }

    [Fact]
    public void Email_Throws_When_Missing()
    {
        var (sut, _) = Build();
        var ex = Assert.Throws<AuthenticationException>(() => _ = sut.Email);
        Assert.Equal("User's Email is null", ex.Message);
    }

    // ---------- EstablishmentId ----------

    [Fact]
    public void EstablishmentId_Parses_Int_When_Present()
    {
        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "42") });
        Assert.Equal(42, sut.ActiveEstablishmentId);
    }

    [Fact]
    public void EstablishmentId_Is_Null_When_Missing_Or_NonNumeric()
    {
        var (sut1, _) = Build();
        Assert.Null(sut1.ActiveEstablishmentId);

        var (sut2, _) = Build(new[] { BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "not-an-int") });
        Assert.Null(sut2.ActiveEstablishmentId);
    }

    // ---------- UserId ----------

    [Fact]
    public void UserId_Parses_Int_When_Present()
    {
        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.DB_USER_ID, "7") });
        Assert.Equal(7, sut.UserId);
    }

    [Fact]
    public void UserId_Is_Null_When_Missing()
    {
        var (sut, _) = Build();
        Assert.Null(sut.UserId);
    }

    // ---------- IsAuthenticated ----------

    [Fact]
    public void IsAuthenticated_True_When_Identity_Authenticated()
    {
        var (sut, _) = Build(authenticated: true);
        Assert.True(sut.IsAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_False_When_No_Auth()
    {
        var (sut, _) = Build(authenticated: false);
        Assert.False(sut.IsAuthenticated);
    }

    // ---------- UserOrganisation properties + IsMat ----------

    [Fact]
    public void UserOrganisation_Properties_Parse_From_DirectEstablishment_Json_Claim()
    {
        // Arrange - using realistic JSON from DSI for a direct establishment (Miscellaneous school)
        // Note: Establishments have URN (not UID)
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "DSI TEST Establishment (001) Miscellanenous (27)",
          "category": {
            "id": "001",
            "name": "Establishment"
          },
          "type": {
            "id": "27",
            "name": "Miscellaneous"
          },
          "urn": "123456",
          "uid": null,
          "ukprn": "00000018"
        }
        """;

        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.Organisation, orgJson) });

        // Act / Assert - verify all UserOrganisation properties are mapped correctly
        Assert.Equal(Guid.Parse("CC1185B8-3142-4B6C-887C-ADC413CD3891"), sut.UserOrganisationDsiId);
        Assert.Equal("DSI TEST Establishment (001) Miscellanenous (27)", sut.UserOrganisationName);
        Assert.Equal("123456", sut.UserOrganisationUrn); // URN is for establishments only
        Assert.Equal("00000018", sut.UserOrganisationUkprn);
        Assert.Null(sut.UserOrganisationUid); // UID is null for establishments (groups only)
        Assert.Equal("Miscellaneous", sut.UserOrganisationTypeName); // From type.name
        Assert.False(sut.IsMat); // Category is "001" (Establishment), not "010" (MAT)
    }

    [Fact]
    public void UserOrganisation_Properties_Parse_From_MAT_Json_Claim_And_IsMat_True()
    {
        // Arrange - using realistic JSON from DSI for a Multi-Academy Trust
        // Note: MATs (establishment groups) have UID (not URN), and don't have a "type" property
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "DSI TEST Multi-Academy Trust (010)",
          "category": {
            "id": "010",
            "name": "Multi-Academy Trust"
          },
          "urn": null,
          "uid": "9876",
          "upin": "100004",
          "ukprn": "00000046"
        }
        """;

        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.Organisation, orgJson) });

        // Act / Assert - verify all UserOrganisation properties are mapped correctly
        Assert.Equal(Guid.Parse("D9011C85-F851-4746-B4A2-D732536717F8"), sut.UserOrganisationDsiId);
        Assert.Equal("DSI TEST Multi-Academy Trust (010)", sut.UserOrganisationName);
        Assert.Null(sut.UserOrganisationUrn); // URN is null for groups (establishments only)
        Assert.Equal("00000046", sut.UserOrganisationUkprn);
        Assert.Equal("9876", sut.UserOrganisationUid); // UID is for establishment groups only
        Assert.Null(sut.UserOrganisationTypeName); // MATs don't have a "type" property
        Assert.True(sut.IsMat); // Category is "010" (Multi-Academy Trust)
    }

    [Fact]
    public void UserOrganisationReference_Returns_First_Valid_Reference()
    {
        // Test URN as reference when all identifiers present
        var orgWithUrn = new EstablishmentModel
        {
            Id = Guid.NewGuid(),
            Urn = "12345",
            Ukprn = "67890",
            Uid = "99999"
        };
        var json1 = JsonSerializer.Serialize(orgWithUrn);
        var (sut1, _) = Build(new[] { BuildClaim(ClaimConstants.Organisation, json1) });
        Assert.Equal("12345", sut1.UserOrganisationReference);

        // Test UKPRN as fallback reference when URN is null
        var orgWithUkprn = new EstablishmentModel
        {
            Id = Guid.NewGuid(),
            Urn = null,
            Ukprn = "67890",
            Uid = "99999"
        };
        var json2 = JsonSerializer.Serialize(orgWithUkprn);
        var (sut2, _) = Build(new[] { BuildClaim(ClaimConstants.Organisation, json2) });
        Assert.Equal("67890", sut2.UserOrganisationReference);

        // Test UID as fallback reference when URN and UKPRN are null
        var orgWithUid = new EstablishmentModel
        {
            Id = Guid.NewGuid(),
            Urn = null,
            Ukprn = null,
            Uid = "99999"
        };
        var json3 = JsonSerializer.Serialize(orgWithUid);
        var (sut3, _) = Build(new[] { BuildClaim(ClaimConstants.Organisation, json3) });
        Assert.Equal("99999", sut3.UserOrganisationReference);

        // Test DSI ID as fallback reference when URN, UKPRN, and UID are null
        var orgWithDsiId = new EstablishmentModel
        {
            Id = Guid.NewGuid(),
            Urn = null,
            Ukprn = null,
            Uid = null
        };
        var json4 = JsonSerializer.Serialize(orgWithDsiId);
        var (sut4, _) = Build(new[] { BuildClaim(ClaimConstants.Organisation, json4) });
        Assert.Equal(orgWithDsiId.Id.ToString(), sut4.UserOrganisationReference);
    }

    [Fact]
    public void UserOrganisation_Properties_When_ClaimsNotPresent_AreNull()
    {
        // Arrange - no Organisation claim
        var (sut, _) = Build();

        // Act / Assert
        Assert.Null(sut.UserOrganisationDsiId);
        Assert.Null(sut.UserOrganisationName);
        Assert.Null(sut.UserOrganisationUrn);
        Assert.Null(sut.UserOrganisationUkprn);
        Assert.Null(sut.UserOrganisationUid);
        Assert.Null(sut.UserOrganisationTypeName);
        Assert.Null(sut.UserOrganisationReference);
    }

    [Fact]
    public void IsMat_When_ClaimsNotPresent_Then_IsFalse()
    {
        // Arrange - no Organisation claim
        var (sut, _) = Build();

        // Act / Assert
        Assert.False(sut.IsMat);
    }

    // ---------- Roles ----------

    [Fact]
    public void IsInRole_Reflects_Principal_Roles()
    {
        var id = new ClaimsIdentity("auth");
        id.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        var principal = new ClaimsPrincipal(id);

        var ctx = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = ctx };
        var sut = new CurrentUser(accessor, Substitute.For<IEstablishmentService>(), Substitute.For<ILogger<CurrentUser>>());

        Assert.True(sut.IsInRole("Admin"));
        Assert.False(sut.IsInRole("Other"));
    }

    // ---------- GroupSelectedSchool cookie ----------

    [Fact]
    public void GetGroupSelectedSchool_Reads_From_Request_Cookies()
    {
        var (sut, ctx) = Build();

        // Simulate an incoming request cookie
        ctx.Request.Headers["Cookie"] = "SelectedSchoolUrn=12345";

        Assert.Equal("12345", sut.GetGroupSelectedSchool());
    }

    [Fact]
    public void SetGroupSelectedSchool_Writes_Response_Cookie_And_Throws_On_Empty()
    {
        var (sut, ctx) = Build();

        // empty -> throws
        Assert.Throws<InvalidDataException>(() => sut.SetGroupSelectedSchool(""));

        // valid -> writes Set-Cookie
        sut.SetGroupSelectedSchool("99999");
        var setCookie = ctx.Response.Headers["Set-Cookie"].ToString();

        Assert.Contains("SelectedSchoolUrn=99999", setCookie);
        Assert.Contains("httponly", setCookie);
        Assert.Contains("secure", setCookie);
        Assert.Contains("samesite=strict", setCookie.ToLowerInvariant());
    }

}
