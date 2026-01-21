using System.Security.Authentication;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Extensions;
using Dfe.PlanTech.Web.Context.Interfaces;

namespace Dfe.PlanTech.Web.Context;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IEstablishmentService _establishmentService;
    private readonly ILogger<CurrentUser> _logger;
    private readonly Lazy<Task<SqlEstablishmentDto?>> _selectedSchoolLazy;

    public CurrentUser(IHttpContextAccessor contextAccessor, IEstablishmentService establishmentService, ILogger<CurrentUser> logger)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _establishmentService = establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _selectedSchoolLazy = new Lazy<Task<SqlEstablishmentDto?>>(() => LoadSelectedSchoolAsync());
    }

    public string? DsiReference => GetStringFromClaim(ClaimConstants.NameIdentifier)
                                   ?? throw new AuthenticationException("User is not authenticated");

    public string? Email => GetNameIdentifierFromClaim(ClaimConstants.VerifiedEmail)
                            ?? throw new AuthenticationException($"User's {nameof(Email)} is null");


    public string? GroupSelectedSchoolUrn => GetGroupSelectedSchool()?.Urn;

    public string? GroupSelectedSchoolName => GetGroupSelectedSchool()?.Name; // TODO: understand how/if relates to `GetActiveEstablishmentNameAsync` and if we might be able/willing to remove `GroupSelectedSchoolName` (this is from the cookie, versus `GetActiveEstablishmentNameAsync` is from the database?)

    // Active Establishment methods - resolve to selected school for MAT users, otherwise Organisation
    public async Task<string?> GetActiveEstablishmentNameAsync()
    {
        var selectedSchool = await _selectedSchoolLazy.Value;
        return selectedSchool?.OrgName ?? Organisation?.Name;
    }

    public async Task<int?> GetActiveEstablishmentIdAsync()
    {
        var selectedSchool = await _selectedSchoolLazy.Value;
        return selectedSchool?.Id ?? GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);
    }

    public async Task<string?> GetActiveEstablishmentUrnAsync()
    {
        var selectedSchool = await _selectedSchoolLazy.Value;
        return selectedSchool?.EstablishmentRef ?? Organisation?.Urn;
    }

    public string? GetActiveEstablishmentUkprn()
    {
        // Only fall back to Organisation if no school is selected (i.e., user is a direct establishment user)
        // If a school is selected but doesn't have UKPRN, return null
        if (GroupSelectedSchoolUrn != null)
        {
            return null; // Selected establishment doesn't have UKPRN in SqlEstablishmentDto
        }
        return Organisation?.Ukprn;
    }

    public string? GetActiveEstablishmentUid()
    {
        // Only fall back to Organisation if no school is selected (i.e., user is a direct establishment user)
        // If a school is selected but doesn't have UID, return null
        if (GroupSelectedSchoolUrn != null)
        {
            return null; // Selected establishment doesn't have UID in SqlEstablishmentDto
        }
        return Organisation?.Uid;
    }

    public Guid? GetActiveEstablishmentDsiId()
    {
        // Only fall back to Organisation if no school is selected (i.e., user is a direct establishment user)
        // If a school is selected, we don't have the DSI ID for it
        if (GroupSelectedSchoolUrn != null)
        {
            return null; // Selected establishment doesn't have DSI ID
        }
        return Organisation?.Id;
    }

    public string? GetActiveEstablishmentReference()
    {
        // Use the selected school URN if available (MAT user has selected a school)
        // Otherwise fall back to the user's organisation reference
        if (GroupSelectedSchoolUrn != null)
        {
            return GroupSelectedSchoolUrn;
        }
        return Organisation?.Reference;
    }

    // User Organisation properties - the organisation the currently logged in user is linked to (from OIDC claims)
    // For direct establishment users, these match ActiveEstablishment properties
    // For MAT users, these represent the MAT/group they belong to
    public string? UserOrganisationName => Organisation?.Name;

    // Note: `DB_ESTABLISHMENT_ID` is the ID of the logged in user's organisation, not the selected establishment
    public int? UserOrganisationId => GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);

    public string? UserOrganisationUrn => Organisation?.Urn;

    public string? UserOrganisationUkprn => Organisation?.Ukprn;

    public string? UserOrganisationUid => Organisation?.Uid;

    public Guid? UserOrganisationDsiId => Organisation?.Id;

    public string? UserOrganisationReference => Organisation?.Reference;

    public string? UserOrganisationTypeName => Organisation?.Type?.Name;

    public string? UserOrganisationCategoryName => Organisation?.Category?.Name;

    public bool UserOrganisationIsGroup => Organisation != null &&
                                           DsiConstants.OrganisationGroupCategories.Contains(Organisation.Category?.Id ?? string.Empty);

    public bool IsAuthenticated => GetIsAuthenticated();

    public bool IsMat => Organisation?.Category?.Id.Equals(DsiConstants.MatOrganisationCategoryId) ?? false;

    private EstablishmentModel? Organisation => _contextAccessor.HttpContext?.User.Claims.GetOrganisation();

    public int? UserId => GetIntFromClaim(ClaimConstants.DB_USER_ID);

    public bool IsInRole(string role) => _contextAccessor.HttpContext?.User.IsInRole(role) ?? false;

    public void SetGroupSelectedSchool(string selectedSchoolUrn, string selectedSchoolName)
    {
        if (string.IsNullOrEmpty(selectedSchoolUrn) || string.IsNullOrEmpty(selectedSchoolName))
        {
            throw new InvalidDataException("No Urn/School name set for selection.");
        }

        var schoolData = new SelectedSchoolCookieData
        {
            Urn = selectedSchoolUrn,
            Name = selectedSchoolName
        };

        var schoolDataJson = System.Text.Json.JsonSerializer.Serialize(schoolData);

        _contextAccessor.HttpContext?.Response.Cookies.Delete(CookieConstants.SelectedSchool);

        _contextAccessor.HttpContext?.Response.Cookies.Append(CookieConstants.SelectedSchool, schoolDataJson, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });
    }

    public (string Urn, string Name)? GetGroupSelectedSchool()
    {
        var httpContext = _contextAccessor.HttpContext;

        if (httpContext == null ||
            !httpContext.Request.Cookies.TryGetValue(CookieConstants.SelectedSchool, out var cookieValue) ||
            string.IsNullOrWhiteSpace(cookieValue))
        {
            return null;
        }

        try
        {
            var school = System.Text.Json.JsonSerializer.Deserialize<SelectedSchoolCookieData>(cookieValue);
            if (school != null && (!string.IsNullOrEmpty(school.Urn) && !string.IsNullOrEmpty(school.Name)))
            {
                return (school.Urn, school.Name);
            }
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }

        return null;
    }

    // Private instance method to load selected school asynchronously
    private async Task<SqlEstablishmentDto?> LoadSelectedSchoolAsync()
    {
        var httpContext = _contextAccessor.HttpContext;

        // Early return if user is not authenticated
        if (!IsAuthenticated)
        {
            return null;
        }

        // Early return if user is not a group user - they shouldn't have a selected school cookie
        if (!UserOrganisationIsGroup)
        {
            // Non-group users should not have a selected school cookie - clear it if present
            if (httpContext?.Request.Cookies.ContainsKey(CookieConstants.SelectedSchool) == true)
            {
                _logger.LogWarning(
                    "Non-group user has school selection cookie but should not. Clearing school selection cookie ({CookieName}).",
                    CookieConstants.SelectedSchool);
                ClearSelectedSchoolCookie(httpContext);
            }
            return null;
        }

        // From here on, we know the user is a group user
        if (httpContext == null ||
            !httpContext.Request.Cookies.TryGetValue(CookieConstants.SelectedSchool, out var cookieValue) ||
            string.IsNullOrWhiteSpace(cookieValue))
        {
            return null;
        }

        string? urn;
        try
        {
            var school = System.Text.Json.JsonSerializer.Deserialize<SelectedSchoolCookieData>(cookieValue);
            urn = school?.Urn;
            if (string.IsNullOrWhiteSpace(urn))
            {
                _logger.LogWarning(
                    "School selection cookie is missing URN value. Cookie length: {CookieLength}. Clearing school selection cookie ({CookieName}).",
                    cookieValue.Length,
                    CookieConstants.SelectedSchool);
                ClearSelectedSchoolCookie(httpContext);
                return null;
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogWarning(ex,
                "School selection cookie contains invalid JSON. Cookie length: {CookieLength}. Clearing school selection cookie ({CookieName}).",
                cookieValue.Length,
                CookieConstants.SelectedSchool);
            ClearSelectedSchoolCookie(httpContext);
            return null;
        }

        // Validate that the selected school is within the user's group
        if (UserOrganisationId == null)
        {
            _logger.LogWarning(
                "Group user attempted to access establishment {SelectedUrn} but has no organisation ID. Clearing school selection cookie ({CookieName}).",
                urn,
                CookieConstants.SelectedSchool);
            ClearSelectedSchoolCookie(httpContext);
            return null;
        }

        try
        {
            // Get all schools in the user's group
            var groupSchools = await _establishmentService.GetEstablishmentLinksWithRecommendationCounts(UserOrganisationId.Value);

            var selectedSchoolIsValid = groupSchools.Any(s => s.Urn.Equals(urn, StringComparison.OrdinalIgnoreCase));

            if (!selectedSchoolIsValid)
            {
                _logger.LogWarning(
                    "Group user attempted to access establishment {SelectedUrn} that is not within their group (GID: {GroupId}). Clearing school selection cookie ({CookieName}).",
                    urn,
                    UserOrganisationId.Value,
                    CookieConstants.SelectedSchool);
                ClearSelectedSchoolCookie(httpContext);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to validate group membership for URN {Urn} in group {GroupId}. Clearing school selection cookie ({CookieName}).",
                urn,
                UserOrganisationId.Value,
                CookieConstants.SelectedSchool);
            ClearSelectedSchoolCookie(httpContext);
            return null;
        }

        try
        {
            return await _establishmentService.GetEstablishmentByReferenceAsync(urn);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get establishment details for URN {Urn} from the database: {Message}. Clearing school selection cookie ({CookieName}).",
                urn, ex.Message,
                CookieConstants.SelectedSchool);
            return null;
        }
    }

    private static void ClearSelectedSchoolCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(CookieConstants.SelectedSchool);
    }

    private int? GetIntFromClaim(string claimType)
    {
        return int.TryParse(GetNameIdentifierFromClaim(claimType), out var id) ? id : null;
    }

    private bool GetIsAuthenticated()
    {
        return _contextAccessor.HttpContext?.User.GetAuthorisationStatus().IsAuthenticated == true;
    }

    private string? GetNameIdentifierFromClaim(string claimType)
    {
        return _contextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    }

    private string? GetStringFromClaim(string claimType)
    {
        return _contextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Contains(claimType))?.Value;
    }
}
