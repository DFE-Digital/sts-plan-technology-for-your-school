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
    private readonly ILogger<CurrentUserProvider> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpContextAccessor _contextAccessor =
        contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly IGiasService _giasService =
        giasService ?? throw new ArgumentNullException(nameof(giasService));

    public string DsiReference =>
        GetStringFromClaim(ClaimConstants.NameIdentifier)
        ?? throw new AuthenticationException("User is not authenticated");

    public Guid? SessionId =>
        Guid.TryParse(GetNameIdentifierFromClaim(ClaimConstants.SessionId), out var sessionId)
            ? sessionId
            : null;

    public int UserId =>
        GetIntFromClaim(ClaimConstants.DB_USER_ID)
        ?? throw new AuthenticationException("User ID missing from claims");

    public SqlEstablishmentDto Establishment =>
        Session.GetUserEstablishment()
        ?? throw new InvalidDataException("User has no establishment");

    public void SetEstablishment(SqlEstablishmentDto establishment)
    {
        Session.SetUserEstablishment(establishment);
    }

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

    public void ClearFocusedSchool()
    {
        if (DsiOrganisation?.Category?.Id != DsiConstants.MatOrganisationCategoryId)
        {
            throw new InvalidOperationException("Only MATs can clear their focused school");
        }

        Session.ClearFocusedEstablishment();
    }

    /// <summary>
    /// Get the focused school for the current user if set, otherwise run actionIsNull if set,
    /// otherwise return null.
    /// </summary>
    /// <param name="actionIfNull">The action to execute if the value is null</param>
    /// <returns>The focused school for the user</returns>
    public SelectedEstablishmentDataModel GetFocusedSchool()
    {
        var focusedEstablishment = Session.GetFocusedEstablishment();
        if (!CanSelectSchools && focusedEstablishment is null)
        {
            throw new InvalidOperationException("Non-group user's focused school is null");
        }

        return Session.GetFocusedEstablishment()
            ?? throw new InvalidDataException("There is no focused school present in the session");
    }

    public async Task SetFocusedSchoolAsync(string urn, string name)
    {
        if (string.IsNullOrEmpty(urn) || string.IsNullOrEmpty(name))
        {
            throw new InvalidDataException("No URN/school name set for selection.");
        }

        if (CanSelectSchools && !GetGroupSchools().Any(gs => gs.Urn.Equals(urn)))
        {
            throw new InvalidOperationException(
                "User attempted to set a focused school that is not in their group"
            );
        }

        // Optimisation: Throw before calling the database if session is null.
        var session = Session;

        var establishment = await _establishmentService.GetOrCreateSchoolAsync(urn, name);
        session.SetFocusedEstablishment(
            establishment.Id,
            establishment.EstablishmentRef,
            establishment.OrgName!
        );
    }

    public List<SelectedEstablishmentDataModel> GetGroupSchools()
    {
        if (!CanSelectSchools)
        {
            throw new InvalidOperationException(
                "Attempted to get group schools for a non-group user"
            );
        }

        return Session.GetGroupSchools().ToList();
    }

    public async Task SetGroupSchools(IEnumerable<SelectedEstablishmentDataModel> schools)
    {
        if (schools.Any(s => s.Urn is null))
        {
            throw new InvalidDataException("All group schools must have a URN");
        }

        var urns = schools.Select(s => s.Urn!);
        var allUrnsValid = AllUrnsValid(urns);
        if (!allUrnsValid)
        {
            throw new InvalidDataException(
                "Group user attempted to access a school that is not within their group."
            );
        }

        // Optimisation: Throw before calling the database if session is null.
        var session = Session;

        var establishments = await _establishmentService.GetEstablishmentsByReferencesAsync(urns);
        if (urns.Count() != establishments.Count())
        {
            throw new InvalidDataException(
                "One or more establishments do not exist in the database."
            );
        }

        session.SetGroupSchools(schools);
    }

    public void ClearSelectedSchools()
    {
        Session.ClearSelectedEstablishments();
    }

    public List<int> GetSelectedSchoolEstablishmentIds()
    {
        return GetSelectedSchools().Select(e => e.EstablishmentId).ToList();
    }

    public List<string> GetSelectedSchoolNames()
    {
        return GetSelectedSchools().Select(e => e.Name).ToList();
    }

    /// <summary>
    /// Get the selected schools for the current user if set, otherwise run actionIsNull if set,
    /// otherwise return an empty list.
    /// </summary>
    /// <param name="actionIfNull">The action to execute if the value is null</param>
    /// <returns>The selected schools for the user</returns>
    public List<SelectedEstablishmentDataModel> GetSelectedSchools()
    {
        if (!CanSelectSchools)
        {
            throw new InvalidOperationException(
                "Attempted to get selected schools for a non-group user"
            );
        }

        var selectedSchools = Session.GetSelectedSchools();
        return selectedSchools.Count == 0
            ? throw new InvalidOperationException("User has not selected any schools")
            : selectedSchools;
    }

    public void SetSelectedSchools(SelectedEstablishmentDataModel establishment)
    {
        SetSelectedSchools([establishment]);
    }

    public void SetSelectedSchools(IEnumerable<SelectedEstablishmentDataModel> establishments)
    {
        if (!CanSelectSchools)
        {
            throw new InvalidOperationException("User cannot set selected establishments");
        }

        var session = Session;
        if (establishments.Count() == 0)
        {
            session.ClearSelectedEstablishments();
        }

        var urns = establishments.Select(e => e.Urn);
        var allUrnsValid = AllUrnsValid(urns);
        if (!allUrnsValid)
        {
            _logger.LogInformation("Clearing selected establishments.");
            session.ClearSelectedEstablishments();
        }

        session.SetSelectedSchools(establishments);
    }

    private bool OrganisationCategoryIdMatchesAny(IEnumerable<string> categoryIds)
    {
        return DsiOrganisation?.Category?.Id != null
            && categoryIds.Contains(DsiOrganisation.Category.Id);
    }

    private bool AllUrnsValid(IEnumerable<string> urns)
    {
        var selectedUrns = "[" + string.Join(", ", urns) + "]";

        var groupEstablishmentId = Establishment.Id;
        var groupUid = DsiOrganisation?.GroupUid ?? Establishment.GroupUid;
        if (groupUid is null)
        {
            _logger.LogWarning(
                "Group user attempted to access establishments {SelectedUrns} but the user's Organisation Group UID is empty and the service cannot validate that the schools are within the user's group.",
                selectedUrns
            );
            return false;
        }

        var groupSchools = Session.GetGroupSchools();
        var allSchoolsInGroup = groupSchools.Select(gs => gs.Urn).ToHashSet().IsSupersetOf(urns);

        if (!allSchoolsInGroup)
        {
            _logger.LogWarning(
                "Group user attempted to access establishments {SelectedUrns}, but at least one is not within their group (group establishment ID: {GroupId}).",
                selectedUrns,
                groupEstablishmentId
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
