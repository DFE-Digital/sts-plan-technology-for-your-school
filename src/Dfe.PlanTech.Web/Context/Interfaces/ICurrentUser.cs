namespace Dfe.PlanTech.Web.Context.Interfaces;

public interface ICurrentUser
{
    string? DsiReference { get; }
    string? Email { get; }
    string? GroupSelectedSchoolUrn { get; }
    string? GroupSelectedSchoolName { get; }

    // Active Establishment - the selected establishment (if MAT user has selected one), otherwise the user's organisation
    Task<string?> GetActiveEstablishmentNameAsync();
    Task<int?> GetActiveEstablishmentIdAsync();
    Task<string?> GetActiveEstablishmentUrnAsync();
    Task<string?> GetActiveEstablishmentUkprnAsync();
    Task<string?> GetActiveEstablishmentUidAsync();
    Task<Guid?> GetActiveEstablishmentDsiIdAsync();
    Task<string?> GetActiveEstablishmentReferenceAsync();

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
    bool UserOrganisationIsGroup { get; }

    bool IsAuthenticated { get; }
    bool IsMat { get; }
    int? UserId { get; }
    (string Urn, string Name)? GetGroupSelectedSchool();
    bool IsInRole(string role);
    void SetGroupSelectedSchool(string selectedSchoolUrn, string selectedSchoolName);
}
