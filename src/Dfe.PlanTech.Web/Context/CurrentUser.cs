using System.Security.Authentication;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Extensions;
using Dfe.PlanTech.Web.Context.Interfaces;

namespace Dfe.PlanTech.Web.Context;

public class CurrentUser(IHttpContextAccessor contextAccessor, IEstablishmentService establishmentService) : ICurrentUser
{
    private readonly IHttpContextAccessor _contextAccessor =
        contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    private readonly IEstablishmentService _establishmentService = establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private SqlEstablishmentDto? _cachedSelectedSchool;
    private bool _selectedSchoolLoaded;

    public string? DsiReference => GetStringFromClaim(ClaimConstants.NameIdentifier)
                                   ?? throw new AuthenticationException("User is not authenticated");

    public string? Email => GetNameIdentifierFromClaim(ClaimConstants.VerifiedEmail)
                            ?? throw new AuthenticationException($"User's {nameof(Email)} is null");

    public int? EstablishmentId => GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);

    public string? GroupSelectedSchoolUrn => GetGroupSelectedSchool();

    // Active Establishment properties - resolve to selected school for MAT users, otherwise Organisation
    public string? ActiveEstablishmentName => GetActiveEstablishmentName();

    public int? ActiveEstablishmentId => GetActiveEstablishmentId();

    public string? ActiveEstablishmentUrn => GetActiveEstablishmentUrn();

    public string? ActiveEstablishmentUkprn => GetActiveEstablishmentUkprn();

    public string? ActiveEstablishmentUid => GetActiveEstablishmentUid();

    public Guid? ActiveEstablishmentDsiId => GetActiveEstablishmentDsiId();

    public string? ActiveEstablishmentReference => GroupSelectedSchoolUrn ?? Organisation?.Reference;

    // User Organisation properties - the organisation the currently logged in user is linked to (from OIDC claims)
    // For direct establishment users, these match ActiveEstablishment properties
    // For MAT users, these represent the MAT/group they belong to
    public string? UserOrganisationName => Organisation?.Name;

    public int? UserOrganisationId => EstablishmentId;

    public string? UserOrganisationUrn => Organisation?.Urn;

    public string? UserOrganisationUkprn => Organisation?.Ukprn;

    public string? UserOrganisationUid => Organisation?.Uid;

    public Guid? UserOrganisationDsiId => Organisation?.Id;

    public string? UserOrganisationReference => Organisation?.Reference;

    public string? UserOrganisationTypeName => Organisation?.Type?.Name;

    private static HashSet<string> OrganisationGroupCategories { get; } = new()
    {
        DsiConstants.MatOrganisationCategoryId,
        // DsiConstants.SatOrganisationCategoryId,
        // DsiConstants.SSatOrganisationCategoryId,
    };
    public bool UserOrganisationIsGroup => Organisation != null &&
        OrganisationGroupCategories.Contains(Organisation.Category?.Id ?? string.Empty);

    public bool IsAuthenticated => GetIsAuthenticated();

    public bool IsMat => Organisation?.Category?.Id.Equals(DsiConstants.MatOrganisationCategoryId) ?? false;

    public int? MatEstablishmentId => GetIntFromClaim(ClaimConstants.DB_MAT_ESTABLISHMENT_ID);

    public EstablishmentModel? Organisation => _contextAccessor.HttpContext?.User.Claims.GetOrganisation();

    public int? UserId => GetIntFromClaim(ClaimConstants.DB_USER_ID);

    public bool IsInRole(string role) => _contextAccessor.HttpContext?.User.IsInRole(role) ?? false;

    public void SetGroupSelectedSchool(string selectedSchoolUrn)
    {
        if (string.IsNullOrEmpty(selectedSchoolUrn))
        {
            throw new InvalidDataException("No Urn for selection");
        }

        _contextAccessor.HttpContext?.Response.Cookies.Delete("SelectedSchoolUrn");

        _contextAccessor.HttpContext?.Response.Cookies.Append("SelectedSchoolUrn", selectedSchoolUrn, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });

        // Clear cache when selection changes
        _selectedSchoolLoaded = false;
        _cachedSelectedSchool = null;
    }

    public string? GetGroupSelectedSchool()
    {
        var httpContext = _contextAccessor.HttpContext;

        if (httpContext != null &&
            httpContext.Request.Cookies.TryGetValue("SelectedSchoolUrn", out var selectedSchoolUrn))
        {
            return selectedSchoolUrn;
        }

        return null;
    }

    // Private methods for Active Establishment resolution
    private SqlEstablishmentDto? GetSelectedSchoolDetails()
    {
        if (_selectedSchoolLoaded)
        {
            return _cachedSelectedSchool;
        }

        _selectedSchoolLoaded = true;

        if (GroupSelectedSchoolUrn == null)
        {
            return null;
        }

        try
        {
            // Synchronously get the selected establishment details
            // This is acceptable as it's only called once per request and cached
            _cachedSelectedSchool = _establishmentService.GetLatestSelectedGroupSchoolAsync(GroupSelectedSchoolUrn)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception)
        {
            // Log warning and fall back to user's organisation details
            _cachedSelectedSchool = null;
        }

        return _cachedSelectedSchool;
    }

    private string? GetActiveEstablishmentName()
    {
        var selectedSchool = GetSelectedSchoolDetails();
        return selectedSchool?.OrgName ?? Organisation?.Name;
    }

    private int? GetActiveEstablishmentId()
    {
        var selectedSchool = GetSelectedSchoolDetails();
        return selectedSchool?.Id ?? EstablishmentId;
    }

    private string? GetActiveEstablishmentUrn()
    {
        var selectedSchool = GetSelectedSchoolDetails();
        return selectedSchool?.EstablishmentRef ?? Organisation?.Urn;
    }

    private string? GetActiveEstablishmentUkprn()
    {
        // Only fall back to Organisation if no school is selected (i.e., user is a direct establishment user)
        // If a school is selected but doesn't have UKPRN, return null
        if (GroupSelectedSchoolUrn != null)
        {
            return null; // Selected establishment doesn't have UKPRN in SqlEstablishmentDto
        }
        return Organisation?.Ukprn;
    }

    private string? GetActiveEstablishmentUid()
    {
        // Only fall back to Organisation if no school is selected (i.e., user is a direct establishment user)
        // If a school is selected but doesn't have UID, return null
        if (GroupSelectedSchoolUrn != null)
        {
            return null; // Selected establishment doesn't have UID in SqlEstablishmentDto
        }
        return Organisation?.Uid;
    }

    private Guid? GetActiveEstablishmentDsiId()
    {
        // Only fall back to Organisation if no school is selected (i.e., user is a direct establishment user)
        // If a school is selected, we don't have the DSI ID for it
        if (GroupSelectedSchoolUrn != null)
        {
            return null; // Selected establishment doesn't have DSI ID
        }
        return Organisation?.Id;
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
