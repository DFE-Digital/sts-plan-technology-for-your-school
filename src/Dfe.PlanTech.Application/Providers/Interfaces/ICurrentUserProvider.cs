using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Providers.Interfaces;

public interface ICurrentUserProvider
{
    string? DsiReference { get; }
    string? GroupSelectedSchoolUrn { get; }
    string? GroupSelectedSchoolName { get; }
    Guid? SessionId { get; }
    int? UserId { get; }

    // Active Establishment - the selected establishment (if MAT user has selected one), otherwise the user's organisation
    Task<string?> GetActiveEstablishmentNameAsync();
    Task<int?> GetActiveEstablishmentIdAsync();
    Task<string?> GetActiveEstablishmentUrnAsync();
    string? GetActiveEstablishmentUkprn();
    string? GetActiveEstablishmentUid();
    Guid? GetActiveEstablishmentDsiId();
    string? GetActiveEstablishmentReference();

    // User Organisation - the organisation the currently logged in user is linked to (from OIDC claims)
    // For direct establishment users, these match ActiveEstablishment properties
    // For MAT users, these represent the MAT/group, not the selected establishment
    string? UserOrganisationName { get; }
    int? UserOrganisationId { get; }
    string? UserOrganisationUrn { get; }
    string? UserOrganisationUkprn { get; }
    string? UserOrganisationUid { get; }
    Guid? UserOrganisationDsiId { get; }
    string? UserOrganisationReference { get; }
    string? UserOrganisationTypeName { get; }
    string? UserOrganisationCategoryName { get; }
    bool UserOrganisationSelectsSchools { get; }

    bool IsAuthenticated { get; }
    bool IsGroup { get; }
    bool IsMat { get; }
    bool IsSatOrSSat { get; }
    bool IsInRole(string role);

    bool OrganisationCategoryIdMatchesAny(IEnumerable<string> categoryIds);

    void ClearFocusedAndSelectedEstablishments();
    SelectedEstablishmentDataModel? GetFocusedSchool();
    Task SetFocusedSchoolAsync(string selectedSchoolUrn, string selectedSchoolName);
    IReadOnlyList<int> GetSelectedEstablishmentIds();
    IReadOnlyList<string> GetSelectedEstablishmentNames();
    IReadOnlyList<SelectedEstablishmentDataModel> GetSelectedEstablishments();
    void SetSelectedEstablishments(int id, string urn, string name);
    void SetSelectedEstablishments(SelectedEstablishmentDataModel establishment);
    void SetSelectedEstablishments(IList<SelectedEstablishmentDataModel> establishments);
}
