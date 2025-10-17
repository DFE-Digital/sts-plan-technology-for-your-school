using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.Context.Interfaces;

public interface ICurrentUser
{
    string? DsiReference { get; }
    string? Email { get; }
    int? EstablishmentId { get; }
    string? GroupSelectedSchoolUrn { get; }
    string? GroupSelectedSchoolName  { get; }
    bool IsAuthenticated { get; }
    bool IsMat { get; }
    int? MatEstablishmentId { get; }
    EstablishmentModel? Organisation { get; }
    int? UserId { get; }
    (string Urn, string Name)? GetGroupSelectedSchool();
    bool IsInRole(string role);
    void SetGroupSelectedSchool(string selectedSchoolUrn, string selectedSchoolName);
}
