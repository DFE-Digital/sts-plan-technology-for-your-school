using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
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
        var (sut, _) = Build([BuildClaim(ClaimConstants.NameIdentifier, "dsi-123")]);
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
        var (sut, _) = Build([BuildClaim(ClaimConstants.VerifiedEmail, "user@example.com")]);
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
    public async Task GetActiveEstablishmentIdAsync_ForDirectEstablishmentUser_ReturnsIdFromClaim()
    {
        // Arrange - Direct establishment user (no selected school)
        var (sut, _) = Build([BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "42")]);

        // Act
        var result = await sut.GetActiveEstablishmentIdAsync();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetActiveEstablishmentIdAsync_ForGroupUserWithSelectedSchool_ReturnsSelectedSchoolIdFromDatabase()
    {
        // Arrange - MAT user with selected school
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100") // MAT's ID
        ], "test"));
        context.User = principal;

        // Set selected school cookie
        var schoolData = new { Urn = "999888", Name = "Selected Academy" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();

        // Mock the validation - confirm the school is in the MAT's group
        establishmentService.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(new List<Core.DataTransferObjects.Sql.SqlEstablishmentLinkDto>
            {
                new() { Urn = "999888", EstablishmentName = "Selected Academy", Id = 42 }
            });

        establishmentService.GetEstablishmentByReferenceAsync("999888")
            .Returns(new Core.DataTransferObjects.Sql.SqlEstablishmentDto
            {
                Id = 42, // Selected school's DB ID
                OrgName = "Selected Academy",
                EstablishmentRef = "999888"
            });

        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var result = await sut.GetActiveEstablishmentIdAsync();

        // Assert - Should return selected school's ID (42), not MAT's ID (100)
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetActiveEstablishmentIdAsync_WhenNoClaimAndNoSelectedSchool_ReturnsNull()
    {
        var (sut1, _) = Build();
        Assert.Null(await sut1.GetActiveEstablishmentIdAsync());
    }

    [Fact]
    public async Task GetActiveEstablishmentIdAsync_WhenClaimIsNonNumeric_ReturnsNull()
    {
        var (sut2, _) = Build([BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "not-an-int")]);
        Assert.Null(await sut2.GetActiveEstablishmentIdAsync());
    }

    // ---------- UserId ----------

    [Fact]
    public void UserId_Parses_Int_When_Present()
    {
        var (sut, _) = Build([BuildClaim(ClaimConstants.DB_USER_ID, "7")]);
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

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

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

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

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
        var (sut1, _) = Build([BuildClaim(ClaimConstants.Organisation, json1)]);
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
        var (sut2, _) = Build([BuildClaim(ClaimConstants.Organisation, json2)]);
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
        var (sut3, _) = Build([BuildClaim(ClaimConstants.Organisation, json3)]);
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
        var (sut4, _) = Build([BuildClaim(ClaimConstants.Organisation, json4)]);
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

    [Fact]
    public void GetGroupSelectedSchool_Reads_From_Request_Cookies()
    {
        var (sut, ctx) = Build();

        var schoolData = new { Urn = "12345", Name = "Test school" };
        var json = JsonSerializer.Serialize(schoolData);
        var encoded = Uri.EscapeDataString(json);

        // Set the Cookie header correctly
        ctx.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={encoded}");

        var result = sut.GetGroupSelectedSchool();

        Assert.Equal(("12345", "Test school"), result);
        Assert.Equal("12345", sut.GroupSelectedSchoolUrn);
        Assert.Equal("Test school", sut.GroupSelectedSchoolName);
    }

    [Fact]
    public void GetGroupSelectedSchool_ReturnsNull_WhenIncorrectDataModel()
    {
        var (sut, ctx) = Build();

        var schoolData = new { Id = "12345", School = "Test school" };
        var json = JsonSerializer.Serialize(schoolData);
        var encoded = Uri.EscapeDataString(json);

        // Set the Cookie header correctly
        ctx.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={encoded}");

        var result = sut.GetGroupSelectedSchool();
        Assert.Null(result);
    }

    [Fact]
    public void GetGroupSelectedSchool_ReturnsNull_WhenMalformedJson()
    {
        var (sut, ctx) = Build();

        var malformedJson = "{\"Urn\":\"12345\",\"Name\":\"Test school\"";
        var encoded = Uri.EscapeDataString(malformedJson);

        // Set the Cookie header correctly
        ctx.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={encoded}");

        var result = sut.GetGroupSelectedSchool();
        Assert.Null(result);
    }

    [Fact]
    public void SetGroupSelectedSchool_Writes_Response_Cookie_And_Throws_On_Empty()
    {
        var (sut, ctx) = Build();

        // valid -> writes Set-Cookie
        sut.SetGroupSelectedSchool("99999", "Test school");

        var setCookies = ctx.Response.Headers[HeaderNames.SetCookie];

        Assert.True(setCookies.Count > 0);

        var cookie = setCookies.LastOrDefault(v =>
            v is not null &&
            v.StartsWith("SelectedSchool=", StringComparison.OrdinalIgnoreCase) &&
            !v.Contains("01 Jan 1970", StringComparison.OrdinalIgnoreCase));

        Assert.False(string.IsNullOrEmpty(cookie), $"Expected an appended SelectedSchool cookie.");

        var lower = cookie!.ToLowerInvariant();
        Assert.Contains("httponly", lower);
        Assert.Contains("secure", lower);
        Assert.Contains("samesite=strict", lower);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SetGroupSelectedSchool_ThrowsInvalidDataException_WhenUrnIsNullOrEmpty(string? urn)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(httpContextAccessor, establishmentService, logger);

        // Act & Assert
        var ex = Assert.Throws<InvalidDataException>(() => sut.SetGroupSelectedSchool(urn!, "Test School"));

        Assert.Equal("No Urn/School name set for selection.", ex.Message);
    }

    // ---------- GetActiveEstablishmentNameAsync ----------

    [Fact]
    public async Task GetActiveEstablishmentNameAsync_ForDirectSchoolUser_ReturnsSchoolName()
    {
        // Arrange - Direct school user (logged in directly as a school)
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "Test Primary School",
          "category": { "id": "001", "name": "Establishment" },
          "type": { "id": "27", "name": "Miscellaneous" },
          "urn": "123456"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = await sut.GetActiveEstablishmentNameAsync();

        // Assert
        Assert.Equal("Test Primary School", result);
    }

    [Fact]
    public async Task GetActiveEstablishmentNameAsync_ForGroupUserWithNoSchoolSelected_ReturnsGroupName()
    {
        // Arrange - MAT user who has not yet selected a school (will be prompted to select)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should return MAT name since no school selected
        Assert.Equal("Test Multi-Academy Trust", result);
    }

    [Fact]
    public async Task GetActiveEstablishmentNameAsync_ForGroupUserWithSelectedSchool_ReturnsSelectedSchoolNameFromDatabase()
    {
        // Arrange - MAT user who has selected a school from their group
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100") // MAT's ID
        ], "test"));
        context.User = principal;

        // Set selected school cookie
        var schoolData = new { Urn = "999888", Name = "Selected Academy" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();

        // Mock the validation - confirm the school is in the MAT's group
        establishmentService.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(new List<Core.DataTransferObjects.Sql.SqlEstablishmentLinkDto>
            {
                new() { Urn = "999888", EstablishmentName = "Selected Academy", Id = 42 }
            });

        establishmentService.GetEstablishmentByReferenceAsync("999888")
            .Returns(new Core.DataTransferObjects.Sql.SqlEstablishmentDto
            {
                Id = 42,
                OrgName = "Selected Academy from DB",
                EstablishmentRef = "999888"
            });

        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var result = await sut.GetActiveEstablishmentNameAsync();

        // Assert - should return name from database, not cookie
        Assert.Equal("Selected Academy from DB", result);
    }

    [Fact]
    public async Task GetActiveEstablishmentNameAsync_ForGroupUserWithSelectedSchool_WhenDatabaseLookupFails_FallsBackToGroupName()
    {
        // Arrange - MAT user who has selected a school, but database lookup fails
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([BuildClaim(ClaimConstants.Organisation, orgJson)], "test"));
        context.User = principal;

        // Set selected school cookie
        var schoolData = new { Urn = "999888", Name = "Selected Academy" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        establishmentService.GetEstablishmentByReferenceAsync("999888")
            .Returns(Task.FromException<Core.DataTransferObjects.Sql.SqlEstablishmentDto?>(new Exception("Database error")));

        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var result = await sut.GetActiveEstablishmentNameAsync();

        // Assert - should fall back to MAT/group name
        Assert.Equal("Test Multi-Academy Trust", result);
    }

    // ---------- GetActiveEstablishmentUrnAsync ----------

    [Fact]
    public async Task GetActiveEstablishmentUrnAsync_ForDirectSchoolUser_ReturnsSchoolUrn()
    {
        // Arrange - Direct school user (logged in directly as a school)
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "Test Primary School",
          "category": { "id": "001", "name": "Establishment" },
          "urn": "123456"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = await sut.GetActiveEstablishmentUrnAsync();

        // Assert
        Assert.Equal("123456", result);
    }

    [Fact]
    public async Task GetActiveEstablishmentUrnAsync_ForGroupUserWithNoSchoolSelected_ReturnsNull()
    {
        // Arrange - MAT user who has not yet selected a school (will be prompted to select)
        // Business Rule: MATs don't have URN (only UID), so return null
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" },
          "uid": "9876"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = await sut.GetActiveEstablishmentUrnAsync();

        // Assert - MAT doesn't have URN, only schools do
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveEstablishmentUrnAsync_ForGroupUserWithSelectedSchool_ReturnsSelectedSchoolUrn()
    {
        // Arrange - MAT user who has selected a school from their group
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100") // MAT's ID
        ], "test"));
        context.User = principal;

        // Set selected school cookie
        var schoolData = new { Urn = "999888", Name = "Selected Academy" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();

        // Mock the validation - confirm the school is in the MAT's group
        establishmentService.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(
            [
                new() { Urn = "999888", EstablishmentName = "Selected Academy", Id = 42 }
            ]);

        establishmentService.GetEstablishmentByReferenceAsync("999888")
            .Returns(new SqlEstablishmentDto
            {
                Id = 42,
                OrgName = "Selected Academy",
                EstablishmentRef = "999888"
            });

        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var result = await sut.GetActiveEstablishmentUrnAsync();

        // Assert
        Assert.Equal("999888", result);
    }

    // ---------- GetActiveEstablishmentUkprnAsync ----------

    [Fact]
    public void GetActiveEstablishmentUkprn_ForDirectSchoolUser_ReturnsSchoolUkprn()
    {
        // Arrange - Direct school user (logged in directly as a school)
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "Test Primary School",
          "category": { "id": "001", "name": "Establishment" },
          "urn": "123456",
          "ukprn": "10012345"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = sut.GetActiveEstablishmentUkprn();

        // Assert
        Assert.Equal("10012345", result);
    }

    [Fact]
    public void GetActiveEstablishmentUkprn_ForGroupUserWithNoSchoolSelected_ReturnsGroupUkprn()
    {
        // Arrange - MAT user who has not yet selected a school (will be prompted to select)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" },
          "ukprn": "10067890"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = sut.GetActiveEstablishmentUkprn();

        // Assert - Should return MAT's UKPRN since no school selected
        Assert.Equal("10067890", result);
    }

    [Fact]
    public void GetActiveEstablishmentUkprn_ForGroupUserWithSelectedSchool_ReturnsNull()
    {
        // Arrange - MAT user who has selected a school from their group
        // Business Rule: Schools do (sometimes) have an UKPRN on DSI, but we do not store it in our database,
        // therefore we cannot return it here for a group user (the user is not logged in directly as the school).
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" },
          "ukprn": "10067890"
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([BuildClaim(ClaimConstants.Organisation, orgJson)], "test"));
        context.User = principal;

        // Set selected school cookie
        var schoolData = new { Urn = "999888", Name = "Selected Academy" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var result = sut.GetActiveEstablishmentUkprn();

        // Assert - Should return null because selected establishment doesn't have UKPRN in the database
        Assert.Null(result);
    }

    // ---------- GetActiveEstablishmentUid ----------

    [Fact]
    public void GetActiveEstablishmentUid_ForDirectSchoolUser_ReturnsNull()
    {
        // Arrange - Direct school user (logged in directly as a school)
        // Note: Schools don't have UID (only groups like MATs and SATs have a UID)
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "Test Primary School",
          "category": { "id": "001", "name": "Establishment" },
          "urn": "123456"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = sut.GetActiveEstablishmentUid();

        // Assert - Schools don't have UID
        Assert.Null(result);
    }

    [Fact]
    public void GetActiveEstablishmentUid_ForGroupUserWithNoSchoolSelected_ReturnsGroupUid()
    {
        // Arrange - MAT user (logged in as MAT, no school selected) - MATs have UID
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" },
          "uid": "9876"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = sut.GetActiveEstablishmentUid();

        // Assert
        Assert.Equal("9876", result);
    }

    [Fact]
    public void GetActiveEstablishmentUid_ForGroupUserWithSelectedSchool_ReturnsNull()
    {
        // Arrange - MAT user who has selected a school from their group
        // Note: Schools do not have a UID (UIDs are for establishment groups only),
        // therefore we expect null for the "active" school when a group user has selected one
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" },
          "uid": "9876"
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([BuildClaim(ClaimConstants.Organisation, orgJson)], "test"));
        context.User = principal;

        // Set selected school cookie
        var schoolData = new { Urn = "999888", Name = "Selected Academy" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var result = sut.GetActiveEstablishmentUid();

        // Assert - Should return null because selected schools don't have UID in the database
        Assert.Null(result);
    }

    // ---------- GetActiveEstablishmentDsiId ----------

    [Fact]
    public void GetActiveEstablishmentDsiId_ForDirectSchoolUser_ReturnsSchoolDsiId()
    {
        // Arrange - Direct school user (logged in directly as a school)
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "Test Primary School",
          "category": { "id": "001", "name": "Establishment" }
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = sut.GetActiveEstablishmentDsiId();

        // Assert
        Assert.Equal(Guid.Parse("CC1185B8-3142-4B6C-887C-ADC413CD3891"), result);
    }

    [Fact]
    public void GetActiveEstablishmentDsiId_ForGroupUserWithNoSchoolSelected_ReturnsGroupDsiId()
    {
        // Arrange - MAT user who has not yet selected a school (will be prompted to select)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = sut.GetActiveEstablishmentDsiId();

        // Assert - Should return MAT's DSI ID since no school selected
        Assert.Equal(Guid.Parse("D9011C85-F851-4746-B4A2-D732536717F8"), result);
    }

    [Fact]
    public void GetActiveEstablishmentDsiId_ForGroupUserWithSelectedSchool_ReturnsNull()
    {
        // Arrange - MAT user who has selected a school from their group
        // Note: Schools do have an organisation ID on DSI, but we do not store it in our database,
        // therefore we cannot return it here for a group user (the user is not logged in directly as the school).
        var orgJson = """
                      {
                        "id": "D9011C85-F851-4746-B4A2-D732536717F8",
                        "name": "Test Multi-Academy Trust",
                        "category": { "id": "010", "name": "Multi-Academy Trust" }
                      }
                      """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([BuildClaim(ClaimConstants.Organisation, orgJson)], "test"));
        context.User = principal;

        // Set selected school cookie
        var schoolData = new { Urn = "999888", Name = "Selected Academy" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var result = sut.GetActiveEstablishmentDsiId();

        // Assert - Should return null because we don't have DSI ID for selected schools
        Assert.Null(result);
    }

    // ---------- GetActiveEstablishmentReference ----------

    [Fact]
    public void GetActiveEstablishmentReference_ForDirectSchoolUser_ReturnsSchoolUrn()
    {
        // Arrange - Direct school user (logged in directly as a school with URN)
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "Test Primary School",
          "category": { "id": "001", "name": "Establishment" },
          "urn": "123456"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = sut.GetActiveEstablishmentReference();

        // Assert - Should use URN as reference
        Assert.Equal("123456", result);
    }

    [Fact]
    public void GetActiveEstablishmentReference_ForGroupUserWithNoSchoolSelected_ReturnsGroupReference()
    {
        // Arrange - MAT user who has not yet selected a school (will be prompted to select)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" },
          "uid": "9876"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act
        var result = sut.GetActiveEstablishmentReference();

        // Assert - Should return MAT's reference (UID) since no school selected
        Assert.Equal("9876", result);
    }

    [Fact]
    public void GetActiveEstablishmentReference_ForGroupUserWithSelectedSchool_ReturnsSelectedSchoolUrn()
    {
        // Arrange - MAT user who has selected a school from their group
        var orgJson = """
                      {
                        "id": "D9011C85-F851-4746-B4A2-D732536717F8",
                        "name": "Test Multi-Academy Trust",
                        "category": { "id": "010", "name": "Multi-Academy Trust" },
                        "uid": "9876"
                      }
                      """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([BuildClaim(ClaimConstants.Organisation, orgJson)], "test"));
        context.User = principal;

        // Set selected school cookie
        var schoolData = new { Urn = "999888", Name = "Selected Academy" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var result = sut.GetActiveEstablishmentReference();

        // Assert - Should return selected school URN, not MAT's UID
        Assert.Equal("999888", result);
    }

    // ---------- UserOrganisationIsGroup ----------

    [Fact]
    public void UserOrganisationIsGroup_WhenMatOrganisation_ReturnsTrue()
    {
        // Arrange - MAT organisation
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "Test Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act / Assert
        Assert.True(sut.UserOrganisationIsGroup);
    }

    // [Fact]
    // public void UserOrganisationIsGroup_WhenSatOrganisation_ReturnsTrue()
    // {
    //     // Arrange - SAT (Single Academy Trust) organisation
    //     var orgJson = """
    //     {
    //       "id": "A1011C85-F851-4746-B4A2-D732536717F8",
    //       "name": "Test Single Academy Trust",
    //       "category": { "id": "013", "name": "Single-Academy Trust" }
    //     }
    //     """;
    //
    //     var (sut, _) = Build([{ BuildClaim(ClaimConstants.Organisation, orgJson) }]);
    //
    //     // Act / Assert
    //     Assert.True(sut.UserOrganisationIsGroup);
    // }

    // [Fact]
    // public void UserOrganisationIsGroup_WhenSsatOrganisation_ReturnsTrue()
    // {
    //     // Arrange - SSAT organisation
    //     var orgJson = """
    //     {
    //       "id": "B2011C85-F851-4746-B4A2-D732536717F8",
    //       "name": "Test Secure Single Academy Trust",
    //       "category": { "id": "014", "name": "Secure Single-Academy Trust" }
    //     }
    //     """;
    //
    //     var (sut, _) = Build([{ BuildClaim(ClaimConstants.Organisation, orgJson) }]);
    //
    //     // Act / Assert
    //     Assert.True(sut.UserOrganisationIsGroup);
    // }

    [Fact]
    public void UserOrganisationIsGroup_WhenDirectEstablishment_ReturnsFalse()
    {
        // Arrange - Direct establishment (not a group)
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "Test Primary School",
          "category": { "id": "001", "name": "Establishment" },
          "urn": "123456"
        }
        """;

        var (sut, _) = Build([BuildClaim(ClaimConstants.Organisation, orgJson)]);

        // Act / Assert
        Assert.False(sut.UserOrganisationIsGroup);
    }

    [Fact]
    public void UserOrganisationIsGroup_WhenOrganisationNull_ReturnsFalse()
    {
        // Arrange - No organisation claim
        var (sut, _) = Build();

        // Act / Assert
        Assert.False(sut.UserOrganisationIsGroup);
    }

    // ---------- LoadSelectedSchool validation (security tests) ----------

    [Fact]
    public async Task NonGroupUser_CannotAccessSchool_WhenCookieIsManipulated()
    {
        // Arrange - Direct school user with URN 123456
        var orgJson = """
        {
          "id": "CC1185B8-3142-4B6C-887C-ADC413CD3891",
          "name": "My Primary School",
          "category": { "id": "001", "name": "Establishment" },
          "urn": "123456"
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100")
        ], "test"));
        context.User = principal;

        // Bad actor: User manually adds a selected school cookie for a different school
        var maliciousSchoolData = new { Urn = "999888", Name = "Someone Elses School" };
        var json = JsonSerializer.Serialize(maliciousSchoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should ignore the manipulated cookie and return the user's actual school
        Assert.Equal(100, activeEstablishmentId); // Their actual school ID from claims
        Assert.Equal("123456", activeEstablishmentUrn); // Their actual school URN
        Assert.Equal("My Primary School", activeEstablishmentName);

        // Verify the cookie was cleared (warning logged)
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Non-group user has school selection cookie but should not")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Verify we never called the establishment service (short-circuited early)
        await establishmentService.DidNotReceive().GetEstablishmentByReferenceAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GroupUser_CannotAccessSchoolOutsideTheirGroup_WhenCookieIsManipulated()
    {
        // Arrange - MAT user (group ID 100) who tries to access a school not in their group
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100") // MAT's DB ID
        ], "test"));
        context.User = principal;

        // Bad actor: User manually edits cookie to try accessing a school outside their MAT
        var maliciousSchoolData = new { Urn = "888777", Name = "School From Different MAT" };
        var json = JsonSerializer.Serialize(maliciousSchoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();

        // Setup: The MAT only has schools with URNs 111222 and 333444
        establishmentService.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(new List<SqlEstablishmentLinkDto>
            {
                new() { Urn = "111222", EstablishmentName = "My School 1", Id = 1 },
                new() { Urn = "333444", EstablishmentName = "My School 2", Id = 2 }
            });

        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should reject the manipulated cookie and fall back to MAT details
        Assert.Equal(100, activeEstablishmentId); // MAT's ID
        Assert.Null(activeEstablishmentUrn); // MAT doesn't have URN
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify validation was attempted
        await establishmentService.Received(1).GetEstablishmentLinksWithRecommendationCounts(100);

        // Verify the cookie was cleared (warning logged)
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("not within their group")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Verify we never tried to load the malicious school
        await establishmentService.DidNotReceive().GetEstablishmentByReferenceAsync("888777");
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccessSchool_EvenWithValidCookie()
    {
        // Arrange - Unauthenticated user with a valid-looking cookie
        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
        context.User = principal;

        var schoolData = new { Urn = "123456", Name = "Some School" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should return nulls (no access)
        Assert.Null(activeEstablishmentId);
        Assert.Null(activeEstablishmentUrn);
        Assert.Null(activeEstablishmentName);

        // Verify we never called the establishment service
        await establishmentService.DidNotReceive().GetEstablishmentLinksWithRecommendationCounts(Arg.Any<int>());
        await establishmentService.DidNotReceive().GetEstablishmentByReferenceAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GroupUser_CanAccessSchoolWithinTheirGroup_WhenValidCookieIsSet()
    {
        // Arrange - MAT user (group ID 100) with a valid school selection within their group
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100") // MAT's DB ID
        ], "test"));
        context.User = principal;

        // Valid selection: User selects a school within their MAT
        var validSchoolData = new { Urn = "111222", Name = "My School 1" };
        var json = JsonSerializer.Serialize(validSchoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();

        // Setup: The MAT has this school
        establishmentService.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(new List<SqlEstablishmentLinkDto>
            {
                new() { Urn = "111222", EstablishmentName = "My School 1", Id = 42 },
                new() { Urn = "333444", EstablishmentName = "My School 2", Id = 43 }
            });

        // Setup: Return the school details from the database
        establishmentService.GetEstablishmentByReferenceAsync("111222")
            .Returns(new SqlEstablishmentDto
            {
                Id = 42,
                OrgName = "My School 1 (Full Name)",
                EstablishmentRef = "111222"
            });

        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should successfully return the selected school
        Assert.Equal(42, activeEstablishmentId); // Selected school's ID
        Assert.Equal("111222", activeEstablishmentUrn); // Selected school's URN
        Assert.Equal("My School 1 (Full Name)", activeEstablishmentName); // Name from database

        // Verify validation was performed
        await establishmentService.Received(1).GetEstablishmentLinksWithRecommendationCounts(100);

        // Verify the school was loaded from database
        await establishmentService.Received(1).GetEstablishmentByReferenceAsync("111222");

        // Verify no warnings were logged
        logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GroupUser_CannotAccessSchool_WhenValidationServiceThrowsException()
    {
        // Arrange - MAT user with a school selection, but validation fails due to service error
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100")
        ], "test"));
        context.User = principal;

        var schoolData = new { Urn = "111222", Name = "My School 1" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();

        // Setup: Service throws an exception during validation
        establishmentService.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(Task.FromException<List<SqlEstablishmentLinkDto>>(new Exception("Database connection failed")));

        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should fail securely and fall back to MAT details
        Assert.Equal(100, activeEstablishmentId); // MAT's ID
        Assert.Null(activeEstablishmentUrn); // MAT doesn't have URN
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify error was logged
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to validate group membership")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Verify we never tried to load the school after validation failed
        await establishmentService.DidNotReceive().GetEstablishmentByReferenceAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GroupUser_WithNoOrganisationId_CannotAccessSchool()
    {
        // Arrange - Group user without an organisation ID in claims (edge case / data corruption scenario)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson)
            // Note: No DB_ESTABLISHMENT_ID claim
        ], "test"));
        context.User = principal;

        var schoolData = new { Urn = "111222", Name = "My School 1" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should reject and fall back to MAT details
        Assert.Null(activeEstablishmentId); // No ID available
        Assert.Null(activeEstablishmentUrn); // MAT doesn't have URN
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify warning was logged
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("but has no organisation ID")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Verify we never called the establishment service
        await establishmentService.DidNotReceive().GetEstablishmentLinksWithRecommendationCounts(Arg.Any<int>());
    }

    // ---------- Cookie corruption and validation tests ----------

    [Fact]
    public async Task GroupUser_WithMalformedJsonInCookie_CannotAccessSchool()
    {
        // Arrange - Group user with malformed JSON in cookie (e.g., browser corruption, manual editing)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100")
        ], "test"));
        context.User = principal;

        // Set malformed JSON cookie
        var malformedJson = "{\"Urn\":\"123456\",\"Name\":\"Test School\""; // Missing closing brace
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(malformedJson)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should fall back to MAT details
        Assert.Equal(100, activeEstablishmentId); // MAT's ID
        Assert.Null(activeEstablishmentUrn); // MAT doesn't have URN
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify warning was logged with exception details
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("School selection cookie contains invalid JSON")),
            Arg.Is<Exception>(ex => ex is JsonException),
            Arg.Any<Func<object, Exception?, string>>());

        // Verify we never tried to validate or load the school
        await establishmentService.DidNotReceive().GetEstablishmentLinksWithRecommendationCounts(Arg.Any<int>());
        await establishmentService.DidNotReceive().GetEstablishmentByReferenceAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GroupUser_WithEmptyUrnInCookie_CannotAccessSchool()
    {
        // Arrange - Group user with empty URN in cookie (corrupted data or manual editing)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100")
        ], "test"));
        context.User = principal;

        // Set cookie with empty URN
        var schoolData = new { Urn = "", Name = "Test School" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should fall back to MAT details
        Assert.Equal(100, activeEstablishmentId); // MAT's ID
        Assert.Null(activeEstablishmentUrn); // MAT doesn't have URN
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify warning was logged
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("School selection cookie is missing URN value")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Verify we never tried to validate or load the school
        await establishmentService.DidNotReceive().GetEstablishmentLinksWithRecommendationCounts(Arg.Any<int>());
        await establishmentService.DidNotReceive().GetEstablishmentByReferenceAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GroupUser_WithWhitespaceUrnInCookie_CannotAccessSchool()
    {
        // Arrange - Group user with whitespace-only URN in cookie
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100")
        ], "test"));
        context.User = principal;

        // Set cookie with whitespace URN
        var schoolData = new { Urn = "   ", Name = "Test School" };
        var json = JsonSerializer.Serialize(schoolData);
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(json)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should fall back to MAT details
        Assert.Equal(100, activeEstablishmentId);
        Assert.Null(activeEstablishmentUrn);
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify warning was logged
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("School selection cookie is missing URN value")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GroupUser_WithNullUrnInCookie_CannotAccessSchool()
    {
        // Arrange - Group user with null URN in cookie (JSON contains "Urn":null)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100")
        ], "test"));
        context.User = principal;

        // Manually construct JSON with null URN
        var jsonWithNull = "{\"Urn\":null,\"Name\":\"Test School\"}";
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(jsonWithNull)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should fall back to MAT details
        Assert.Equal(100, activeEstablishmentId);
        Assert.Null(activeEstablishmentUrn);
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify warning was logged
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("School selection cookie is missing URN value")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GroupUser_WithCompletelyInvalidCookieValue_CannotAccessSchool()
    {
        // Arrange - Group user with completely invalid cookie value (not even JSON)
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100")
        ], "test"));
        context.User = principal;

        // Set completely invalid cookie value
        var invalidValue = "this-is-not-json-at-all-just-random-text";
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(invalidValue)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentUrn = await sut.GetActiveEstablishmentUrnAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should fall back to MAT details
        Assert.Equal(100, activeEstablishmentId);
        Assert.Null(activeEstablishmentUrn);
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify warning was logged with exception
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("School selection cookie contains invalid JSON")),
            Arg.Is<Exception>(ex => ex is JsonException),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GroupUser_AfterCodeChangeInvalidatesOldCookieFormat_GracefullyHandles()
    {
        // Arrange - Simulates scenario after a code deployment where old cookie format is no longer valid
        // This is a common real-world scenario that should be handled gracefully
        var orgJson = """
        {
          "id": "D9011C85-F851-4746-B4A2-D732536717F8",
          "name": "My Multi-Academy Trust",
          "category": { "id": "010", "name": "Multi-Academy Trust" }
        }
        """;

        var context = new DefaultHttpContext();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            BuildClaim(ClaimConstants.Organisation, orgJson),
            BuildClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "100")
        ], "test"));
        context.User = principal;

        // Old format cookie (different property names)
        var oldFormatJson = "{\"SchoolUrn\":\"123456\",\"SchoolName\":\"Old Format School\"}";
        context.Request.Headers.Append(HeaderNames.Cookie, $"SelectedSchool={Uri.EscapeDataString(oldFormatJson)}");

        var accessor = new HttpContextAccessor { HttpContext = context };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        var sut = new CurrentUser(accessor, establishmentService, logger);

        // Act
        var activeEstablishmentId = await sut.GetActiveEstablishmentIdAsync();
        var activeEstablishmentName = await sut.GetActiveEstablishmentNameAsync();

        // Assert - Should gracefully fall back to MAT without throwing exceptions
        Assert.Equal(100, activeEstablishmentId);
        Assert.Equal("My Multi-Academy Trust", activeEstablishmentName);

        // Verify appropriate warning was logged
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("School selection cookie is missing URN value")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }


}
