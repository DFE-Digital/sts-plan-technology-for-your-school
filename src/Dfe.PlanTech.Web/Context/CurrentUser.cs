using System.Security.Authentication;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Extensions;
using Dfe.PlanTech.Web.Context.Interfaces;

namespace Dfe.PlanTech.Web.Context;

public class CurrentUser(IHttpContextAccessor contextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _contextAccessor =
        contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

    public string? DsiReference => GetStringFromClaim(ClaimConstants.NameIdentifier)
                                   ?? throw new AuthenticationException("User is not authenticated");

    public string? Email => GetNameIdentifierFromClaim(ClaimConstants.VerifiedEmail)
                            ?? throw new AuthenticationException($"User's {nameof(Email)} is null");

    public int? EstablishmentId => GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);

    public string? GroupSelectedSchoolUrn => GetGroupSelectedSchool()?.Urn;

    public string? GroupSelectedSchoolName => GetGroupSelectedSchool()?.Name;

    public bool IsAuthenticated => GetIsAuthenticated();

    public bool IsMat => Organisation?.Category?.Id.Equals(DsiConstants.MatOrganisationCategoryId) ?? false;

    public int? MatEstablishmentId => GetIntFromClaim(ClaimConstants.DB_MAT_ESTABLISHMENT_ID);

    public EstablishmentModel Organisation => _contextAccessor.HttpContext?.User.Claims.GetOrganisation()
            ?? throw new InvalidDataException($"Could not parse user's {nameof(Organisation)} claim");

    public int? UserId => GetIntFromClaim(ClaimConstants.DB_USER_ID);

    public bool IsInRole(string role) => contextAccessor.HttpContext?.User.IsInRole(role) ?? false;

    public void SetGroupSelectedSchool(string selectedSchoolUrn, string selectedSchoolName)
    {
        if (string.IsNullOrEmpty(selectedSchoolUrn))
        {
            throw new InvalidDataException("No Urn for selection");
        }

        var schoolData = new
        {
            Urn = selectedSchoolUrn,
            Name = selectedSchoolName
        };

        var schoolDataJson = System.Text.Json.JsonSerializer.Serialize(schoolData);

        _contextAccessor.HttpContext?.Response.Cookies.Delete("SelectedSchool");

        _contextAccessor.HttpContext?.Response.Cookies.Append("SelectedSchool", schoolDataJson, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });
    }

    public (string Urn, string? Name)? GetGroupSelectedSchool()
    {
        var httpContext = _contextAccessor.HttpContext;

        if (httpContext == null ||
            !httpContext.Request.Cookies.TryGetValue("SelectedSchool", out var cookieValue) ||
            string.IsNullOrWhiteSpace(cookieValue))
        {
            return null;
        }

        try
        {
            var school = System.Text.Json.JsonSerializer.Deserialize<SelectedSchoolCookieData>(cookieValue);
            if (school != null && !string.IsNullOrEmpty(school.Urn))
            {
                return (school.Urn, school.Name);
            }
        }
        catch
        {
            return (cookieValue, null);
        }

        return null;
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
