using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.Context.Interfaces;

public interface ICurrentUser
{
    string? DsiReference { get; }
    string? Email { get; }
    int? EstablishmentId { get; }
    string? GroupSelectedSchoolUrn { get; }
    bool IsAuthenticated { get; }
    bool IsMat { get; }
    int? MatEstablishmentId { get; }
    DsiOrganisationModel Organisation { get; }
    int? UserId { get; }
    string? GetGroupSelectedSchool();
    bool IsInRole(string role);
    void SetGroupSelectedSchool(string selectedSchoolUrn);
}
