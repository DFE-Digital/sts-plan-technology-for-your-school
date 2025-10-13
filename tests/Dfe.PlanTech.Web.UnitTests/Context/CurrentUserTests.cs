using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context;
using Microsoft.AspNetCore.Http;

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
        var sut = new CurrentUser(accessor);
        return (sut, ctx);
    }

    private static Claim BuildClaim(string type, string value) => new(type, value);

    // ---------- ctor guards ----------

    [Fact]
    public void Ctor_Throws_When_ContextAccessor_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new CurrentUser(null!));
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

    // ---------- EstablishmentId / UserId ----------

    [Fact]
    public void EstablishmentId_Parses_Int_When_Present()
    {
        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "42") });
        Assert.Equal(42, sut.EstablishmentId);
    }

    [Fact]
    public void EstablishmentId_Is_Null_When_Missing_Or_NonNumeric()
    {
        var (sut1, _) = Build();
        Assert.Null(sut1.EstablishmentId);

        var (sut2, _) = Build(new[] { BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "not-an-int") });
        Assert.Null(sut2.EstablishmentId);
    }

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

    // ---------- Organisation + IsMat ----------

    [Fact]
    public void Organisation_Parses_From_Json_Claim_And_IsMat_True_When_Category_Matches()
    {
        // Arrange a minimal OrganisationModel with Category.Id = MatOrganisationCategoryId
        var org = new EstablishmentModel
        {
            Id = Guid.NewGuid(),
            Category = new IdWithNameModel { Id = DsiConstants.MatOrganisationCategoryId },
            Urn = "testUrn"
        };
        var json = JsonSerializer.Serialize(org);

        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.Organisation, json) });

        // Act / Assert
        Assert.NotNull(sut.Organisation);
        Assert.Equal(DsiConstants.MatOrganisationCategoryId, sut.Organisation!.Category!.Id);
        Assert.True(sut.IsMat);
    }

    [Fact]
    public void Organisation_Throws_When_Claim_Missing_And_IsMat_Also_Throws()
    {
        var (sut, _) = Build();

        var ex1 = Assert.Throws<InvalidDataException>(() => _ = sut.Organisation);
        Assert.Equal("Could not parse user's Organisation claim", ex1.Message);

        // Because the getter throws, IsMat will propagate that exception too
        var ex2 = Assert.Throws<InvalidDataException>(() => _ = sut.IsMat);
        Assert.Equal("Could not parse user's Organisation claim", ex2.Message);
    }

    [Fact]
    public void Organisation_Throws_When_Json_Invalid()
    {
        var (sut, _) = Build(new[] { BuildClaim(ClaimConstants.Organisation, "{not-json}") });
        var ex = Assert.Throws<JsonException>(() => _ = sut.Organisation);
        Assert.Contains("'n' is an invalid start of a property name", ex.Message);
    }

    [Fact]
    public void Organisation_Throws_When_Json_Null()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Build(new[] { BuildClaim(ClaimConstants.Organisation, null!) }));
        Assert.Contains("Value cannot be null. (Parameter 'value')", ex.Message);
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
        var sut = new CurrentUser(accessor);

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

    // ---------- EstablishmentModel via extension ----------

    [Fact]
    public void GetEstablishmentModel_Throws_When_Not_Present()
    {
        var (sut, _) = Build();
        var ex = Assert.Throws<InvalidDataException>(() => sut.Organisation);
        Assert.Equal("Could not parse user's Organisation claim", ex.Message);
    }
}
