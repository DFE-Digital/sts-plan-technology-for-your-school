using System.Security.Authentication;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Providers;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly ILogger<CurrentUserProvider> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IEstablishmentService _establishmentService;
    private readonly Lazy<Task<SqlEstablishmentDto?>> _selectedSchoolLazy;

    public CurrentUserProvider(
        IHttpContextAccessor contextAccessor,
        IEstablishmentService establishmentService,
        ILogger<CurrentUserProvider> logger
    )
    {
        _contextAccessor =
            contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _establishmentService =
            establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _selectedSchoolLazy = new Lazy<Task<SqlEstablishmentDto?>>(() => LoadSelectedSchoolAsync());
    }

    public string? DsiReference =>
        GetStringFromClaim(ClaimConstants.NameIdentifier)
        ?? throw new AuthenticationException("User is not authenticated");

    public string? GroupSelectedSchoolUrn => GetFocusedSchool()?.Urn;

    public string? GroupSelectedSchoolName => GetFocusedSchool()?.Name; // TODO: understand how/if relates to `GetActiveEstablishmentNameAsync` and if we might be able/willing to remove `GroupSelectedSchoolName` (this is from the cookie, versus `GetActiveEstablishmentNameAsync` is from the database?)

    public Guid? SessionId =>
        Guid.TryParse(GetNameIdentifierFromClaim(ClaimConstants.SessionId), out var sessionId)
            ? sessionId
            : null;

    public int? UserId => GetIntFromClaim(ClaimConstants.DB_USER_ID);

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

    public bool UserOrganisationSelectsSchools =>
        OrganisationCategoryIdMatchesAny(DsiConstants.SelectASchoolOrganisationCategoryIds);

    public bool IsAuthenticated =>
        _contextAccessor.HttpContext?.User.GetAuthorisationStatus().IsAuthenticated == true;

    public bool IsGroup =>
        OrganisationCategoryIdMatchesAny(DsiConstants.GroupOrganisationCategoryIds);

    public bool IsMat => OrganisationCategoryIdMatchesAny([DsiConstants.MatOrganisationCategoryId]);

    public bool IsSatOrSSat =>
        OrganisationCategoryIdMatchesAny(DsiConstants.SatOrganisationCategoryIds);

    public bool IsInRole(string role) => _contextAccessor.HttpContext?.User.IsInRole(role) ?? false;

    public bool OrganisationCategoryIdMatchesAny(IEnumerable<string> categoryIds)
    {
        return Organisation?.Category?.Id != null && categoryIds.Contains(Organisation.Category.Id);
    }

    public void ClearFocusedAndSelectedEstablishments()
    {
        Session.ClearFocusedAndSelectedEstablishments();
    }

    public SelectedEstablishmentDataModel? GetFocusedSchool()
    {
        var session = _contextAccessor.HttpContext?.Session;
        if (session is null)
        {
            throw new InvalidOperationException("Cannot access the current session.");
        }

        return session.GetFocusedEstablishment();
    }

    public async Task SetFocusedSchoolAsync(string urn, string name)
    {
        if (string.IsNullOrEmpty(urn) || string.IsNullOrEmpty(name))
        {
            throw new InvalidDataException("No URN/school name set for selection.");
        }

        // Optimisation: Throw before calling the database if session is null.
        var session = Session;

        var establishment =
            await _establishmentService.GetEstablishmentByReferenceAsync(urn)
            ?? throw new InvalidDataException("No establishment was found matching the given URN.");

        session.SetFocusedEstablishment(establishment.Id, urn, name);
    }

    public IReadOnlyList<int> GetSelectedEstablishmentIds()
    {
        return Session.GetSelectedEstablishmentIds();
    }

    public IReadOnlyList<string> GetSelectedEstablishmentNames()
    {
        return Session.GetSelectedEstablishmentNames();
    }

    public IReadOnlyList<SelectedEstablishmentDataModel> GetSelectedEstablishments()
    {
        return Session.GetSelectedEstablishments();
    }

    public void SetSelectedEstablishments(int id, string urn, string name)
    {
        var establishment = new SelectedEstablishmentDataModel
        {
            Id = id,
            Urn = urn,
            Name = name,
        };
        Session.SetSelectedEstablishments([establishment]);
    }

    public void SetSelectedEstablishments(SelectedEstablishmentDataModel establishment)
    {
        Session.SetSelectedEstablishments([establishment]);
    }

    public void SetSelectedEstablishments(IList<SelectedEstablishmentDataModel> establishments)
    {
        Session.SetSelectedEstablishments(establishments);
    }

    private EstablishmentModel? Organisation =>
        _contextAccessor.HttpContext?.User.Claims.GetOrganisation();

    // Private instance method to load selected school asynchronously
    private async Task<SqlEstablishmentDto?> LoadSelectedSchoolAsync()
    {
        var session = _contextAccessor.HttpContext?.Session;
        if (session is null)
        {
            throw new InvalidOperationException("Cannot access the current session.");
        }

        // Early return if user is not authenticated
        if (!IsAuthenticated)
        {
            return null;
        }

        var focusedSchool = session.GetFocusedEstablishment();

        // Early return if user is not a group user - they shouldn't have a selected school cookie
        if (!IsGroup)
        {
            // Non-group users should not have a selected school cookie - clear it if present
            if (focusedSchool is not null)
            {
                _logger.LogWarning(
                    "Non-group user has school selection cookie but should not. Clearing school selection cookie ({CookieName}).",
                    CookieConstants.SelectedSchool
                );
                session.ClearFocusedEstablishment();
            }
            return null;
        }

        // From here on, we know the user is a group user
        if (focusedSchool is null || string.IsNullOrWhiteSpace(focusedSchool.Urn))
        {
            _logger.LogWarning(
                "Focused school session value is missing URN value. Clearing focused school session value."
            );
            session.ClearFocusedEstablishment();
            return null;
        }

        var urn = focusedSchool.Urn;

        var isValidUrn = await IsValidUrn(session, urn);
        if (!isValidUrn)
        {
            return null;
        }

        try
        {
            return await _establishmentService.GetEstablishmentByReferenceAsync(urn);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to get establishment details for URN {Urn} from the database: {Message}. Clearing focused school session value.",
                urn,
                ex.Message
            );
            return null;
        }
    }

    private async Task<bool> IsValidUrn(ISession session, string urn)
    {
        if (UserOrganisationId is null)
        {
            _logger.LogWarning(
                "Group user attempted to access establishment {SelectedUrn} but has no organisation ID. Clearing focused school session value.",
                urn
            );
            session.ClearFocusedEstablishment();
            return false;
        }

        try
        {
            // Get all schools in the user's group
            var groupSchools =
                await _establishmentService.GetEstablishmentLinksWithRecommendationCounts(
                    UserOrganisationId.Value
                );

            var selectedSchoolIsValid = groupSchools.Any(s =>
                s.Urn.Equals(urn, StringComparison.OrdinalIgnoreCase)
            );

            if (!selectedSchoolIsValid)
            {
                _logger.LogWarning(
                    "Group user attempted to access establishment {SelectedUrn} which is not within their group (GID: {GroupId}). Clearing focused school session value.",
                    urn,
                    UserOrganisationId.Value
                );
                session.ClearFocusedEstablishment();
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to validate group membership for URN {Urn} in group {GroupId}. Clearing focused school session value.",
                urn,
                UserOrganisationId.Value
            );
            session.ClearFocusedEstablishment();
            return false;
        }

        return true;
    }

    private ISession Session =>
        _contextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Cannot access the current session.");

    private int? GetIntFromClaim(string claimType)
    {
        return int.TryParse(GetNameIdentifierFromClaim(claimType), out var id) ? id : null;
    }

    private string? GetNameIdentifierFromClaim(string claimType)
    {
        return _contextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    }

    private string? GetStringFromClaim(string claimType)
    {
        return _contextAccessor
            .HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Contains(claimType))
            ?.Value;
    }
}
