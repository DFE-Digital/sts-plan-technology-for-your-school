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
    private readonly IGiasService _giasService;
    private readonly Lazy<Task<IEnumerable<SelectedEstablishmentDataModel>>> _selectedSchoolLazy;

    public CurrentUserProvider(
        ILogger<CurrentUserProvider> logger,
        IHttpContextAccessor contextAccessor,
        IEstablishmentService establishmentService,
        IGiasService giasService
    )
    {
        _contextAccessor =
            contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _establishmentService =
            establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
        _giasService = giasService ?? throw new ArgumentNullException(nameof(giasService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _selectedSchoolLazy = new Lazy<Task<IEnumerable<SelectedEstablishmentDataModel>>>(
            LoadSelectedSchoolsAsync
        );
    }

    public string? DsiReference =>
        GetStringFromClaim(ClaimConstants.NameIdentifier)
        ?? throw new AuthenticationException("User is not authenticated");

    public Guid? SessionId =>
        Guid.TryParse(GetNameIdentifierFromClaim(ClaimConstants.SessionId), out var sessionId)
            ? sessionId
            : null;

    public int? UserId => GetIntFromClaim(ClaimConstants.DB_USER_ID);

    public SqlEstablishmentDto? UserEstablishment => Session.GetUserEstablishment();

    public EstablishmentModel? UserOrganisation =>
        _contextAccessor.HttpContext?.User.Claims.GetOrganisation();

    public bool IsAuthenticated =>
        _contextAccessor.HttpContext?.User.GetAuthorisationStatus().IsAuthenticated == true;

    public bool IsSchool =>
        OrganisationCategoryIdMatchesAny(DsiConstants.EstablishmentOrganisationCategoryIds);

    public bool IsGroup =>
        OrganisationCategoryIdMatchesAny(DsiConstants.TrustOrganisationCategoryIds);

    public bool IsMat => OrganisationCategoryIdMatchesAny([DsiConstants.MatOrganisationCategoryId]);

    public bool IsSatOrSSat =>
        OrganisationCategoryIdMatchesAny(DsiConstants.SatOrganisationCategoryIds);

    public bool CanSelectSchools =>
        OrganisationCategoryIdMatchesAny(DsiConstants.SelectASchoolOrganisationCategoryIds);

    public void SetEstablishment(SqlEstablishmentDto establishment)
    {
        Session.SetUserEstablishment(establishment);
    }

    public void ClearFocusedSchool()
    {
        if (UserOrganisation?.Category?.Id != DsiConstants.MatOrganisationCategoryId)
        {
            throw new InvalidOperationException("Only MATs can clear their focused school");
        }

        Session.ClearFocusedEstablishment();
    }

    public SelectedEstablishmentDataModel? FocusedSchool => Session.GetFocusedEstablishment();

    public async Task SetFocusedSchoolAsync(string urn, string name)
    {
        if (string.IsNullOrEmpty(urn) || string.IsNullOrEmpty(name))
        {
            throw new InvalidDataException("No URN/school name set for selection.");
        }

        if (CanSelectSchools && !GroupSchools.Any(gs => gs.Urn.Equals(urn)))
        {
            throw new InvalidOperationException(
                "User attempted to set a focused school that is not in their group"
            );
        }

        // Optimisation: Throw before calling the database if session is null.
        var session = Session;

        var establishment =
            await _establishmentService.GetEstablishmentByReferenceAsync(urn)
            ?? throw new InvalidDataException("No establishment was found matching the given URN.");

        session.SetFocusedEstablishment(establishment.Id, urn, name);
    }

    public IReadOnlyList<SelectedEstablishmentDataModel> GroupSchools => Session.GetGroupSchools();

    public async Task SetGroupSchools(IEnumerable<SelectedEstablishmentDataModel> schools)
    {
        if (schools.Any(gs => gs.Urn is null))
        {
            throw new InvalidDataException("All group schools must have a URN");
        }

        var urns = schools.Select(e => e.Urn!);
        var allUrnsValid = await HasSchoolsInGroup(urns);
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

    public void ClearSelectedEstablishments()
    {
        Session.ClearSelectedEstablishments();
    }

    public IReadOnlyList<int> GetSelectedEstablishmentIds()
    {
        return GetSelectedEstablishments().Select(e => e.Id).ToArray();
    }

    public IReadOnlyList<string> GetSelectedEstablishmentNames()
    {
        return GetSelectedEstablishments().Select(e => e.Name).ToArray();
    }

    public IReadOnlyList<SelectedEstablishmentDataModel> GetSelectedEstablishments()
    {
        return Session.GetSelectedEstablishments();
    }

    public void SetSelectedEstablishments(SelectedEstablishmentDataModel establishment)
    {
        Session.SetSelectedEstablishments([establishment]);
    }

    public void SetSelectedEstablishments(
        IEnumerable<SelectedEstablishmentDataModel> establishments
    )
    {
        Session.SetSelectedEstablishments(establishments);
    }

    private bool OrganisationCategoryIdMatchesAny(IEnumerable<string> categoryIds)
    {
        return UserOrganisation?.Category?.Id != null
            && categoryIds.Contains(UserOrganisation.Category.Id);
    }

    // Private instance method to load selected school asynchronously
    private async Task<IEnumerable<SelectedEstablishmentDataModel>> LoadSelectedSchoolsAsync()
    {
        var session = _contextAccessor.HttpContext?.Session;
        if (session is null)
        {
            throw new InvalidOperationException("Cannot access the current session.");
        }

        // Early return if user is not authenticated
        if (!IsAuthenticated)
        {
            return [];
        }

        var selectedSchools = session.GetSelectedEstablishments();

        // Early return if user is not a group user.
        // They shouldn't be able to select schools, so warn and clear it if present.
        if (!CanSelectSchools)
        {
            if (selectedSchools is not null)
            {
                _logger.LogWarning(
                    "Non-group user has selected schools but should not be able to. Clearing selected schools from session ({SessionKey}).",
                    SessionConstants.SelectedEstablishmentsKey
                );

                session.ClearSelectedEstablishments();
            }
            return [];
        }

        // From here on, we know the user should be able to select schools
        if (selectedSchools.Count == 0)
        {
            _logger.LogWarning("Selected schools session value is empty.");

            return [];
        }

        var urns = Session.GetSelectedEstablishments().Select(e => e.Urn);
        var allUrnsValid = await HasSchoolsInGroup(urns);
        if (!allUrnsValid)
        {
            _logger.LogInformation("Clearing selected establishments.");
            session.ClearSelectedEstablishments();
            return [];
        }

        return selectedSchools;
    }

    private async Task<bool> HasSchoolsInGroup(IEnumerable<string> urns)
    {
        var selectedUrns = "[" + string.Join(", ", urns) + "]";

        if (UserOrganisation?.Id is null || UserEstablishment?.Id is null)
        {
            _logger.LogWarning(
                "Group user attempted to access establishments {SelectedUrns} but has no organisation ID or establishment ID.",
                selectedUrns
            );
            return false;
        }

        if (UserOrganisation.GroupUid is null)
        {
            _logger.LogWarning(
                "Group user attempted to access establishments {SelectedUrns} but the user's Organisation Group UID is empty and the service cannot validate that the schools are within the user's group.",
                selectedUrns
            );
            return false;
        }

        var userOrganisationEstablishmentId = UserEstablishment.Id;

        try
        {
            // Get all schools in the user's group
            var allSchoolsInGroup = await _giasService.HasAllSchoolsInGroup(
                UserOrganisation.GroupUid,
                urns
            );
            if (!allSchoolsInGroup)
            {
                _logger.LogWarning(
                    "Group user attempted to access establishments {SelectedUrns}, but at least one is not within their group (group establishment ID: {GroupId}).",
                    selectedUrns,
                    userOrganisationEstablishmentId
                );
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to validate group membership for URNs {SelectedUrns} in group {GroupId}.",
                selectedUrns,
                userOrganisationEstablishmentId
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
