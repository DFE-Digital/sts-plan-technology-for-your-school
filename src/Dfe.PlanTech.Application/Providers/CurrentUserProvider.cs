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

public class CurrentUserProvider(
    ILogger<CurrentUserProvider> logger,
    IHttpContextAccessor contextAccessor,
    IEstablishmentService establishmentService,
    IGiasService giasService
) : ICurrentUserProvider
{
    private readonly ILogger<CurrentUserProvider> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor
        ?? throw new ArgumentNullException(nameof(contextAccessor));
    private readonly IEstablishmentService _establishmentService = establishmentService
        ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly IGiasService _giasService = giasService
        ?? throw new ArgumentNullException(nameof(giasService));

    public string DsiReference =>
        GetStringFromClaim(ClaimConstants.NameIdentifier)
        ?? throw new AuthenticationException("User is not authenticated");

    public int? FocusedEstablishmentId =>
        GetIntFromClaim(ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID);

    public Guid? SessionId => GetGuidFromClaim(ClaimConstants.SessionId);

    public int? TrustEstablishmentId => GetIntFromClaim(ClaimConstants.DB_TRUST_ESTABLISHMENT_ID);

    public int UserEstablishmentId =>
        GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID)
        ?? throw new AuthenticationException("User establishment ID missing from claims");

    public int UserId =>
        GetIntFromClaim(ClaimConstants.DB_USER_ID)
        ?? throw new AuthenticationException("User ID missing from claims");

    // User Organisation - the organisation the currently logged in user is linked to (from OIDC claims)
    // For school users, this is the establishmentId for the school
    // For MAT users, this represents the MAT/group, not the selected establishment
    public EstablishmentModel? DsiOrganisation =>
        _contextAccessor.HttpContext?.User.Claims.GetOrganisation();

    public bool IsAuthenticated =>
        _contextAccessor.HttpContext?.User.GetAuthorisationStatus().IsAuthenticated == true;

    public bool IsSchool =>
        OrganisationCategoryIdMatchesAny(DsiConstants.EstablishmentOrganisationCategoryIds);

    public bool IsMat => OrganisationCategoryIdMatchesAny([DsiConstants.MatOrganisationCategoryId]);

    public bool IsSatOrSSat =>
        OrganisationCategoryIdMatchesAny(DsiConstants.SatOrganisationCategoryIds);

    public bool IsTrust =>
        OrganisationCategoryIdMatchesAny(DsiConstants.TrustOrganisationCategoryIds);

    public bool CanSelectSchools =>
        OrganisationCategoryIdMatchesAny(DsiConstants.SelectASchoolOrganisationCategoryIds);

    public void ClearFocusedEstablishment()
    {
        if (!CanSelectSchools)
        {
            throw new InvalidOperationException(
                "Non-group user attempted to clear their focused school"
            );
        }

        Session.ClearFocusedEstablishment();
    }

    /// <summary>
    /// Get the focused establishment (usually a school) for the current user if set.
    /// Users in establishments that can select schools can get null back, but for other
    /// user types the function throws an exception if the value is null.
    /// </summary>
    /// <returns>The user's focused establishment, if set, otherwise null or an exception.</returns>
    public async Task<SelectedEstablishmentDataModel?> GetFocusedEstablishmentAsync()
    {
        if (!IsAuthenticated)
        {
            return null;
        }

        var session = Session;
        var focusedEstablishment = session.GetFocusedEstablishment();
        if (!CanSelectSchools && focusedEstablishment is null && FocusedEstablishmentId.HasValue)
        {
            var establishment = await _establishmentService.GetEstablishmentByIdAsync(
                FocusedEstablishmentId.Value
            );
            session.SetFocusedEstablishment(
                establishment.Id,
                establishment.EstablishmentRef,
                establishment.OrgName ?? "Unknown name"
            );
            focusedEstablishment = session.GetFocusedEstablishment();
        }

        if (CanSelectSchools)
        {
            // Group users can have no focused school, so don't throw an exception if it's null
            return focusedEstablishment;
        }

        // Non-group users cannot have no focused school, so do throw an exception if it's null
        return focusedEstablishment
            ?? throw new InvalidDataException("There is no focused school present in the session");
    }

    public async Task SetFocusedEstablishmentAsync(string urn, string name)
    {
        if (!CanSelectSchools)
        {
            throw new InvalidOperationException(
                "Non-group user attempted to set their focused school"
            );
        }

        if (string.IsNullOrEmpty(urn) || string.IsNullOrEmpty(name))
        {
            throw new InvalidDataException("No URN/school name set for selection.");
        }

        // If they can select schools...
        var groupSchools = await GetGroupSchoolsAsync();
        var schoolInGroup = groupSchools?.Any(gs => gs.Urn.Equals(urn)) ?? false;
        if (!schoolInGroup)
        {
            throw new InvalidOperationException(
                "User attempted to set a focused school that is not in their group"
            );
        }

        // Optimisation: Throws if session is null before calling the database.
        var session = Session;

        var establishment = await _establishmentService.GetOrCreateSchoolAsync(urn, name);
        session.SetFocusedEstablishment(
            establishment.Id,
            establishment.EstablishmentRef,
            establishment.OrgName!
        );
    }

    /// <summary>
    /// If the user is from a group, get the schools associated with that group's establishment.
    /// For users in establishments which cannot select schools, the function throws an exception.
    /// </summary>
    /// <returns>The group's schools, if set. An exception if they're not a group.</returns>
    public async Task<List<SelectedEstablishmentDataModel>> GetGroupSchoolsAsync()
    {
        if (!IsAuthenticated)
        {
            return [];
        }

        if (!CanSelectSchools)
        {
            throw new InvalidOperationException(
                "Attempted to get group schools for a non-group user"
            );
        }

        var session = Session;
        var groupSchools = session.GetGroupSchools();
        if (groupSchools.Count == 0 && DsiOrganisation?.Uid is not null)
        {
            groupSchools = await _giasService.GetSchoolsForMatAsync(DsiOrganisation);
            await SetGroupSchoolsAsync(groupSchools);
        }

        return groupSchools;
    }

    public async Task SetGroupSchoolsAsync(IEnumerable<SelectedEstablishmentDataModel> schools)
    {
        if (schools.Any(s => string.IsNullOrWhiteSpace(s.Urn) || string.IsNullOrWhiteSpace(s.Name)))
        {
            throw new InvalidDataException("All group schools must have a URN and a name");
        }

        // Optimisation: Throw before calling the database if session is null.
        var session = Session;

        var urns = schools.Select(s => s.Urn);

        var establishments = await _establishmentService.GetEstablishmentsByReferencesAsync(urns);
        var foundEstablishmentUrns = establishments.Select(e => e.EstablishmentRef);
        var missingEstablishmentUrns = schools.Where(s => !foundEstablishmentUrns.Contains(s.Urn));

        var newEstablishments = await _establishmentService.GetOrCreateSchoolsAsync(
            missingEstablishmentUrns
        );
        var totalEstablishmentsInDatabase = establishments.Union(newEstablishments).Count();

        if (urns.Count() != totalEstablishmentsInDatabase)
        {
            throw new InvalidDataException(
                "One or more establishments do not exist in the database."
            );
        }

        session.SetGroupSchools(schools);
    }

    public void ClearSelectedSchools()
    {
        if (CanSelectSchools)
        {
            Session.ClearFocusedEstablishment();
        }

        Session.ClearSelectedEstablishments();
    }

    public List<int>? GetSelectedSchoolEstablishmentIds()
    {
        return GetSelectedSchools()?.Select(e => e.EstablishmentId).ToList();
    }

    public List<string>? GetSelectedSchoolNames()
    {
        return GetSelectedSchools()?.Select(e => e.Name).ToList();
    }

    /// <summary>
    /// Get the selected schools for the current user if set, otherwise run actionIsNull if set,
    /// otherwise return an empty list.
    /// </summary>
    /// <param name="actionIfNull">The action to execute if the value is null</param>
    /// <returns>The selected schools for the user</returns>
    public List<SelectedEstablishmentDataModel>? GetSelectedSchools()
    {
        if (!IsAuthenticated)
        {
            return null;
        }

        if (!CanSelectSchools)
        {
            throw new InvalidOperationException(
                "Attempted to get selected schools for a non-group user"
            );
        }

        return Session.GetSelectedSchools();
    }

    public async Task SetSelectedSchoolsAsync(SelectedEstablishmentDataModel establishment)
    {
        await SetSelectedSchoolsAsync([establishment]);
    }

    public async Task SetSelectedSchoolsAsync(
        IEnumerable<SelectedEstablishmentDataModel> establishments
    )
    {
        if (!CanSelectSchools)
        {
            throw new InvalidOperationException("User cannot set selected establishments");
        }

        var session = Session;
        if (!establishments.Any())
        {
            session.ClearSelectedEstablishments();
            return;
        }

        var urns = establishments.Select(e => e.Urn);
        var allUrnsValid = await AllUrnsValid(urns);
        if (!allUrnsValid)
        {
            session.ClearSelectedEstablishments();
            throw new InvalidOperationException(
                "One or more selected establishments are not in the user's group."
            );
        }

        session.SetSelectedSchools(establishments);
    }

    public async Task<SqlEstablishmentDto> GetUserEstablishmentAsync()
    {
        var session = Session;

        var establishment = session.GetUserEstablishment();
        if (establishment is null)
        {
            establishment = await _establishmentService.GetEstablishmentByIdAsync(
                UserEstablishmentId
            );
            session.SetUserEstablishment(establishment);
        }

        return session.GetUserEstablishment()
            ?? throw new InvalidDataException("There is no establishment present in the session");
    }

    public void SetUserEstablishment(SqlEstablishmentDto establishment)
    {
        Session.SetUserEstablishment(establishment);
    }

    private bool OrganisationCategoryIdMatchesAny(IEnumerable<string> categoryIds)
    {
        return DsiOrganisation?.Category?.Id != null
            && categoryIds.Contains(DsiOrganisation.Category.Id);
    }

    private async Task<bool> AllUrnsValid(IEnumerable<string> urns)
    {
        var selectedUrns = "[" + string.Join(", ", urns) + "]";

        var userEstablishment = await GetUserEstablishmentAsync();
        var groupUid = userEstablishment?.GroupUid ?? DsiOrganisation?.GroupUid;
        if (groupUid is null)
        {
            _logger.LogWarning(
                "Group user attempted to access establishments {SelectedUrns} but the user's Organisation Group UID is empty and the service cannot validate that the schools are within the user's group.",
                selectedUrns
            );
            return false;
        }

        var groupSchools = await GetGroupSchoolsAsync();
        var allSchoolsInGroup = groupSchools.Select(gs => gs.Urn).ToHashSet().IsSupersetOf(urns);

        if (!allSchoolsInGroup)
        {
            _logger.LogWarning(
                "Group user attempted to access establishments {SelectedUrns}, but at least one is not within their group (establishment ID: {UserEstablishmentId}).",
                selectedUrns,
                userEstablishment?.Id
            );
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

    private Guid? GetGuidFromClaim(string claimType)
    {
        return Guid.TryParse(GetNameIdentifierFromClaim(claimType), out var guid) ? guid : null;
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
