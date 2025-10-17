using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.Context.Interfaces;

public interface ICurrentUser
{
    string? DsiReference { get; }
    string? Email { get; }
    int? EstablishmentId { get; }
    string? GroupSelectedSchoolUrn { get; }

    // Active Establishment - the selected establishment (if MAT user has selected one), otherwise the user's organisation
    string? ActiveEstablishmentName { get; }
    int? ActiveEstablishmentId { get; }
    string? ActiveEstablishmentUrn { get; }
    string? ActiveEstablishmentUkprn { get; }
    string? ActiveEstablishmentUid { get; }
    Guid? ActiveEstablishmentDsiId { get; }
    string? ActiveEstablishmentReference { get; }

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

    bool IsAuthenticated { get; }
    bool IsMat { get; }
    int? MatEstablishmentId { get; }
    EstablishmentModel? Organisation { get; }
    int? UserId { get; }
    string? GetGroupSelectedSchool();
    bool IsInRole(string role);
    void SetGroupSelectedSchool(string selectedSchoolUrn);
}
