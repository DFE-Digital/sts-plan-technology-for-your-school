using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.Context
{
    public interface ICurrentUser
    {
        string? DsiReference { get; }
        string? Email { get; }
        int? EstablishmentId { get; }
        string? GroupSelectedSchoolUrn { get; }
        bool IsAuthenticated { get; }
        bool IsMat { get; }
        OrganisationModel? Organisation { get; }
        int? UserId { get; }

        EstablishmentModel GetEstablishmentModel();
        string? GetGroupSelectedSchool();
        bool IsInRole(string role);
        void SetGroupSelectedSchool(string selectedSchoolUrn);
    }
}