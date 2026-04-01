using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Options;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Web.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Web.Controllers;

[NonProductionOnly]
[Route("api/mock-auth")]
public class MockAuthController(
    IEstablishmentRepository establishmentRepository,
    IUserRepository userRepository,
    IMemoryCache cache,
    IOptions<AutomatedTestingConfiguration> options)
    : Controller
{
    private const string SelectorCookieName = "e2e_user";
    private const string SelectorCookieKey = "e2e_key";

    private const string TestSchoolUserId = "E2E_TEST_SCHOOL_USER";
    private const string TestMatUserId = "E2E_TEST_MAT_USER";

    private const string SchoolTestOrgName = "DSI TEST Establishment (001) Miscell";
    private const string MatTestOrgName = "DSI TEST Multi-Academy Trust (010)";

    private static readonly string[] ResponseTypesSupported = ["code"];
    private static readonly string[] SubjectTypesSupported = ["public"];
    private static readonly string[] IdTokenAlgValuesSupported = ["RS256"];

    private static readonly RSA Rsa = RSA.Create(2048);

    private static readonly RsaSecurityKey SecurityKey =
        new(Rsa) { KeyId = "e2e-auth-test-key" };

    private static readonly SigningCredentials SigningCredentials =
        new(SecurityKey, SecurityAlgorithms.RsaSha256);

    private static readonly ConcurrentDictionary<string, DateTime> CodeExpiries = new();

    [HttpGet(".well-known/openid-configuration")]
    public IActionResult Discovery()
    {
        var baseUrl = GetBaseUrl();

        return Ok(new
        {
            issuer = baseUrl,
            authorization_endpoint = $"{baseUrl}/authorize",
            token_endpoint = $"{baseUrl}/token",
            jwks_uri = $"{baseUrl}/jwks",
            response_types_supported = ResponseTypesSupported,
            subject_types_supported = SubjectTypesSupported,
            id_token_signing_alg_values_supported = IdTokenAlgValuesSupported,
            end_session_endpoint = $"{baseUrl}/endsession",

        });
    }

    [HttpGet("jwks")]
    public IActionResult Jwks()
    {
        var parameters = Rsa.ExportParameters(false);

        string Base64Url(byte[] input) =>
            Base64UrlEncoder.Encode(input);

        return Ok(new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    alg = "RS256",
                    kid = SecurityKey.KeyId,
                    n = Base64Url(parameters.Modulus!),
                    e = Base64Url(parameters.Exponent!)
                }
            }
        });
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize(
        [FromQuery] string redirect_uri,
        [FromQuery] string response_type,
        [FromQuery] string state,
        [FromQuery] string? nonce)
    {
        if (!IsAllowed())
        {
            return Content("The service is currently unavailable to use while E2E tests are running.", "text/plain");
        }

        var allowedRedirectUri = GetAllowedAuthorizeRedirectUri();

        if (!string.Equals(redirect_uri, allowedRedirectUri, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid redirect_uri");

        if (response_type != "code")
            return BadRequest("Only authorization_code supported");

        var userType = GetUserTypeFromCookie(Request);
        var fixtureUser = FromSelector(userType);

        var code = Guid.NewGuid().ToString("N");

        var now = DateTime.UtcNow;
        foreach (var kvp in CodeExpiries)
        {
            if (kvp.Value >= now) continue;
            if (CodeExpiries.TryRemove(kvp.Key, out _))
            {
                cache.Remove(kvp.Key);
            }
        }

        var schoolTestEstablishments =
            await establishmentRepository.GetEstablishmentsByAsync(e => e.OrgName == SchoolTestOrgName);
        var schoolTestEstablishment = schoolTestEstablishments.FirstOrDefault();

        var matTestEstablishments =
            await establishmentRepository.GetEstablishmentsByAsync(e => e.OrgName == MatTestOrgName);
        var matTestEstablishment = matTestEstablishments.FirstOrDefault();

        var schoolUser = await userRepository.GetUserBySignInRefAsync(TestSchoolUserId);
        var matUser = await userRepository.GetUserBySignInRefAsync(TestMatUserId);

        var testSchoolEstablishmentId = schoolTestEstablishment?.Id;
        var testMatEstablishmentId = matTestEstablishment?.Id;
        var testSchoolUserId = schoolUser?.Id;
        var testMatUserId = matUser?.Id;

        var record = new AuthCodeRecord
        {
            Subject = fixtureUser.Sub,
            Email = fixtureUser.Email,
            OrganisationJson = fixtureUser.OrganisationJson,
            RedirectUri = redirect_uri,
            Nonce = nonce,
            Expires = DateTime.UtcNow.AddMinutes(2),
            DbUserId = testSchoolUserId ?? throw new InvalidOperationException("testSchoolUserId not set"),
            DbEstablishmentId = testSchoolEstablishmentId ?? throw new InvalidOperationException("testSchoolEstablishmentId not set"),
        };

        if (userType == "mat")
        {
            record.DbEstablishmentId = testMatEstablishmentId ?? throw new InvalidOperationException("testMatEstablishmentId not set");
            record.DbUserId = testMatUserId ?? throw new InvalidOperationException("testMatUserId not set");
        }

        CodeExpiries[code] = record.Expires;
        cache.Set(code, record, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = record.Expires
        });

        var redirect =
            $"{allowedRedirectUri}?code={code}&state={Uri.EscapeDataString(state)}";

        return Redirect(redirect);
    }

    [HttpPost("token")]
    public IActionResult Token(
        [FromForm] string grant_type,
        [FromForm] string code,
        [FromForm] string redirect_uri,
        [FromForm] string client_id)
    {

        var baseUrl = GetBaseUrl();

        if (grant_type != "authorization_code")
            return BadRequest(new { error = "unsupported_grant_type" });

        if (!cache.TryGetValue<AuthCodeRecord>(code, out var record) || record is null)
            return BadRequest(new { error = "invalid_grant" });

        cache.Remove(code);
        CodeExpiries.TryRemove(code, out _);

        if (record.Expires < DateTime.UtcNow)
            return BadRequest(new { error = "expired_code" });

        if (record.RedirectUri != redirect_uri)
            return BadRequest(new { error = "redirect_mismatch" });

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(30);

        var idClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, record.Subject),
            new(JwtRegisteredClaimNames.Email, record.Email),
            new(JwtRegisteredClaimNames.Iss, baseUrl),
            new(JwtRegisteredClaimNames.Aud, client_id),
            new(ClaimConstants.DB_ESTABLISHMENT_ID, record.DbEstablishmentId.ToString()),
            new(ClaimConstants.DB_USER_ID, record.DbUserId.ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(record.Nonce))
            idClaims.Add(new Claim("nonce", record.Nonce));

        if (!string.IsNullOrEmpty(record.OrganisationJson))
            idClaims.Add(new Claim(ClaimConstants.Organisation, record.OrganisationJson));

        var idToken = CreateJwt(baseUrl, idClaims, now, expires);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, record.Subject),
            new(JwtRegisteredClaimNames.Iss, baseUrl),
            new(JwtRegisteredClaimNames.Aud, client_id),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(ClaimConstants.Organisation, record.OrganisationJson ?? ""),
            new("scope", "openid profile email"),
        };

        if (record.DbMatEstablishmentId is not null)
        {
            claims.Add(new(ClaimConstants.DB_MAT_ESTABLISHMENT_ID, record.DbMatEstablishmentId.ToString()!));
        }

        var accessToken = CreateJwt(baseUrl, claims, now, expires);

        return Ok(new
        {
            access_token = accessToken,
            id_token = idToken,
            token_type = "Bearer",
            expires_in = 1800
        });
    }

    [HttpGet("endsession")]
    public IActionResult EndSession(
        [FromQuery] string? state)
    {
        if (!IsAllowed()) return NotFound();

        Response.Cookies.Delete(SelectorCookieName);
        Response.Cookies.Delete(SelectorCookieKey);

        var redirect = "/";


        if (!string.IsNullOrEmpty(state))
        {
            redirect += $"?state={Uri.EscapeDataString(state)}";
        }

        return Redirect(redirect);
    }


    private static string CreateJwt(string baseUrl, IEnumerable<Claim> claims,
        DateTime notBefore, DateTime expires)
    {
        var token = new JwtSecurityToken(
            issuer: baseUrl,
            claims: claims,
            notBefore: notBefore,
            expires: expires,
            signingCredentials: SigningCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class AuthCodeRecord
    {
        public string Subject { get; init; } = null!;
        public string Email { get; init; } = null!;

        public string? OrganisationJson { get; init; }
        public string RedirectUri { get; init; } = null!;
        public string? Nonce { get; init; }
        public DateTime Expires { get; init; }
        public int DbEstablishmentId { get; set; }
        public int DbUserId { get; set; }
        public int? DbMatEstablishmentId { get; set; } = null;
    }

    private static string GetUserTypeFromCookie(HttpRequest request)
    {
        return request.Cookies.TryGetValue(SelectorCookieName, out var v) ? v : "school";
    }

    private static readonly string SchoolOrganisationJson = @"{
      ""id"": ""EED74904-DB35-4A46-AC23-E2E19C95F715"",
      ""name"": ""DSI TEST Establishment (001) Miscellanenous (27)"",
      ""category"": { ""id"": ""001"", ""name"": ""Establishment"" },
      ""type"": { ""id"": ""27"", ""name"": ""Miscellaneous"" },
      ""urn"": ""900008"",
      ""status"": { ""id"": 1, ""name"": ""Open"", ""tagColor"": ""green"" },
      ""legacyId"": ""4000499""
    }";

    private static readonly string MatOrganisationJson = @"{
      ""id"": ""91380A4E-F30F-4FF8-80A1-D37178CD6AB3"",
      ""name"": ""DSI TEST Multi-Academy Trust (010)"",
      ""category"": { ""id"": ""010"", ""name"": ""Multi-Academy Trust"" },
      ""type"": { ""id"": ""99"", ""name"": ""Multi-Academy Trust"" },
      ""ukprn"": ""00000046"",
      ""upin"": ""100004"",
      ""status"": { ""id"": 1, ""name"": ""Open"", ""tagColor"": ""green"" },
      ""legacyId"": ""4000527""
    }";

    public sealed record FixtureUser(
        string Sub,
        string Email,
        string? OrganisationJson
    );

    private static FixtureUser FromSelector(string? selector)
    {
        selector = (selector ?? "school").Trim().ToLowerInvariant();

        return selector switch
        {
            "school" => new FixtureUser(
                Sub: "E2E_TEST_SCHOOL_USER",
                Email: "school@test.local",
                OrganisationJson: SchoolOrganisationJson
            ),

            "mat" => new FixtureUser(
                Sub: "E2E_TEST_MAT_USER",
                Email: "mat@test.local",
                OrganisationJson: MatOrganisationJson
            ),

            "noorg" => new FixtureUser(
                Sub: "E2E_TEST_NOORG_USER",
                Email: "noorg@test.local",
                OrganisationJson: null
            ),

            _ => new FixtureUser(
                Sub: "E2E_TEST_SCHOOL_USER",
                Email: "school@test.local",
                OrganisationJson: SchoolOrganisationJson
            )
        };
    }

    private bool IsAllowed()
    {
        if (options.Value.MockAuthentication?.ClientSecret is null)
        {
            throw new InvalidOperationException("MockAuthentication.ClientSecret is null");
        }

        if (!Request.Cookies.TryGetValue(SelectorCookieKey, out var key))
        {
            return false;
        }

        return key == options.Value.MockAuthentication?.ClientSecret;
    }

    private string GetBaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}/api/mock-auth";
    }

    private string GetAllowedAuthorizeRedirectUri()
    {
        return $"{Request.Scheme}://{Request.Host}/auth/cb";
    }

}
